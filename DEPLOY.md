# Guía de despliegue — Lavanderia 2.0

Documento oficial para desplegar Lavanderia 2.0 en un servidor Linux de producción.

---

## 1. Arquitectura general

```
┌──────────────────────────────────────────────────────────────┐
│                     Producción (Linux ARM64)                │
│                      163.192.142.183                        │
│                                                              │
│   ┌─────────────────┐     ┌─────────────────────────────┐  │
│   │  Nginx (puerto 80) │────▶│  API .NET 8 (puerto 5000)  │  │
│   │  Serving static   │     │  LaundryManagement.API     │  │
│   │  + reverse proxy  │     │                             │  │
│   └─────────────────┘     └─────────────────────────────┘  │
│          │                                                    │
│          │              ┌─────────────────────────────┐      │
│          └─────────────▶│  PostgreSQL 16 (puerto 5432) │      │
│                         │  lavanderiadb               │      │
│                         └─────────────────────────────┘      │
└──────────────────────────────────────────────────────────────┘
```

- **Frontend**: Build estático de React served directamente por Nginx
- **Backend**: API REST .NET 8 autocontenida
- **Base de datos**: PostgreSQL 16 Alpine
- **Proxy inverso**: Nginx sirve el frontend y reenvía `/api/*` al backend

---

## 2. Requisitos del servidor

| Recurso | Mínimo | Recomendado |
|---------|--------|-------------|
| CPU | 1 core | 2+ cores |
| RAM | 1 GB | 2 GB |
| Disco | 10 GB | 20 GB SSD |
| OS | Ubuntu 20.04+ / Debian / AlmaLinux | Ubuntu 22.04 LTS |
| Docker | 20.10+ | latest |
| Docker Compose | 2.0+ | latest |

---

## 3. Preparación del servidor

### 3.1 Docker y Docker Compose

```bash
# Instalar Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Instalar Docker Compose
sudo apt install docker-compose -y
# o
sudo apt install docker.io docker-compose-v2
```

### 3.2 Firewall

```bash
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp     # HTTP (Nginx)
sudo ufw allow 443/tcp    # HTTPS (opcional)
sudo ufw enable
```

---

## 4. Base de datos — PostgreSQL

### 4.1 Levantar PostgreSQL con Docker

```bash
cd /opt/lavanderia
sudo docker compose up -d
```

El archivo `docker-compose.yml` levanta PostgreSQL 16 Alpine con:
- DB: `lavanderiadb`
- Usuario: `lavanderia_user`
- Puerto: `5432`
- Volumen persistente en `/var/lib/docker/volumes/lavanderia_pgdata`

### 4.2 Crear base de datos y usuario

```bash
# Conectarse al contenedor
sudo docker exec -it lavanderia_postgres psql -U postgres -d postgres

# En psql:
CREATE USER lavanderia WITH PASSWORD 'TU_PASSWORD_FUERTE';
CREATE DATABASE lavanderiadb OWNER lavanderia;
GRANT ALL PRIVILEGES ON DATABASE lavanderiadb TO lavanderia;
\q
```

### 4.3 Aplicar esquema y seed

```bash
# En el servidor (con el SQL schema ya copiado)
sudo docker exec -i lavanderia_postgres psql -U lavanderia_user -d lavanderiadb < /opt/lavanderia/Database/01_schema.sql
sudo docker exec -i lavanderia_postgres psql -U lavanderia_user -d lavanderiadb < /opt/lavanderia/Database/02_seed.sql
```

### 4.4 Migración de producción (timestamps)

```bash
# Aplicar la migración que cambia timestamps a without time zone
# (viene en el repo: LaundryDbContextModelSnapshot actualizado)
sudo docker exec -i lavanderia_postgres psql -U lavanderia_user -d lavanderiadb < /opt/lavanderia/Database/03_prod_timestamps.sql

# Si no existe 03_prod_timestamps.sql, verificar con:
dotnet ef migrations script --startup-project ../LaundryManagement.API -o 03_prod_timestamps.sql
```

**Usuarios del seed**:
- `admin` / `Admin123!` — rol admin
- `empleado1` / `Empleado123!` — rol empleado

---

## 5. Backend — API .NET 8

### 5.1 Compilar en本地 (preparación del build)

```bash
# En máquina de desarrollo
cd Backend
dotnet publish -c Release -r linux-arm64 --self-contained false -o publish/
```

Copiar la carpeta `publish/` al servidor en `/opt/lavanderia/backend/`.

### 5.2 Configuración de producción

Los archivos de configuración en `Backend/publish/` son los que se usan en producción. Editar antes de copiar:

**`appsettings.Production.json`**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lavanderiadb;Username=lavanderia_user;Password=TU_PASSWORD_FUERTE"
  },
  "JwtSettings": {
    "SecretKey": "GENERA_UNA_CLAVE_DE_AL_MENOS_64_CHARS_SEGURA",
    "Issuer": "LaundryManagementAPI",
    "Audience": "LaundryManagementClient",
    "ExpirationMinutes": 60
  },
  "AllowedOrigins": [
    "http://163.192.142.183/lavanderia2.0"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "PrinterSettings": {
    "Enabled": true,
    "Type": "WindowsPrinter",
    "PrinterName": "XP-58",
    "PaperWidthChars": 32,
    "BusinessName": "Lavanderia & Tintoreria",
    "FooterLine": "Gracias por su preferencia!"
  },
  "ReportSettings": {
    "Enabled": true,
    "EmailFrom": "deepsoftbot@gmail.com",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "deepsoftbot@gmail.com",
    "SmtpPassword": "TU_GMAIL_APP_PASSWORD",
    "SmtpUseSsl": true
  }
}
```

**Variables de entorno de producción** (alternativa a editar el JSON):

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=lavanderiadb;Username=lavanderia_user;Password=TU_PASSWORD"
export JwtSettings__SecretKey="CLAVE_DE_64_CHARS_MINIMO"
export ReportSettings__SmtpPassword="GMAIL_APP_PASSWORD"
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://*:5000"
```

### 5.3 Script de arranque

```bash
# En el servidor
cd /opt/lavanderia/backend
chmod +x start-production.sh
./start-production.sh
```

`start-production.sh`:
```bash
#!/bin/bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://*:5000"
dotnet LaundryManagement.API.dll
```

### 5.4 systemd (opcional — para auto-inicio)

```bash
sudo nano /etc/systemd/system/lavanderia-api.service
```

```ini
[Unit]
Description=Lavanderia 2.0 API
After=network.target docker.service

[Service]
Type=simple
User=root
WorkingDirectory=/opt/lavanderia/backend
ExecStart=/usr/bin/dotnet /opt/lavanderia/backend/LaundryManagement.API.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://*:5000

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable lavanderia-api
sudo systemctl start lavanderia-api
```

Verificar:
```bash
sudo systemctl status lavanderia-api
curl http://localhost:5000/api/health  # o endpoint de verificación
```

---

## 6. Frontend — Build estático

### 6.1 Variables de entorno de producción

**`Frontend/.env.production`** (antes de build):

```
VITE_AUTH_MODE=api
VITE_API_BASE_URL=http://163.192.142.183/lavanderia2.0/api
```

### 6.2 Compilar

```bash
# En máquina de desarrollo
cd Frontend
npm install
npm run build
```

El build se genera en `Frontend/dist/`. Copiar todo el contenido al servidor en `/var/www/lavanderia/`.

---

## 7. Nginx — Proxy reverso

### 7.1 Instalación

```bash
sudo apt install nginx -y
```

### 7.2 Configuración

```bash
sudo nano /etc/nginx/sites-available/lavanderia
```

```nginx
server {
    listen 80;
    server_name 163.192.142.183;

    root /var/www/lavanderia;
    index index.html;

    # Frontend estático — subdirectorio /lavanderia2.0/
    location /lavanderia2.0/ {
        alias /var/www/lavanderia/;
        try_files $uri $uri/ /lavanderia2.0/index.html;
    }

    # Assets del build
    location /lavanderia2.0/assets/ {
        alias /var/www/lavanderia/assets/;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # API reverse proxy
    location /lavanderia2.0/api/ {
        proxy_pass http://127.0.0.1:5000/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/lavanderia /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### 7.3 HTTPS con Let's Encrypt (opcional)

```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d 163.192.142.183 --deploy-hook "systemctl reload nginx"
```

---

## 8. Seed de permisos actual (v2)

El seed original del `02_seed.sql` tiene permisos con formato antiguo (`Crear_Usuario`). **Para permisos dinámicos granulares**, ejecutar después del seed base:

```bash
sudo docker exec -i lavanderia_postgres psql -U lavanderia_user -d lavanderiadb < /opt/lavanderia/Backend/seed_permissions_v2.sql
```

Esto inserta los permisos con formato `module.section:action`:
- `dashboard.general:view`
- `orders.lista:view`, `orders.lista:export`
- `orders.nueva:create`
- `orders.detalle:view`, `orders.detalle:edit`, `orders.detalle:pay`
- `orders.corte:manage`
- `services.servicios:view`, `services.servicios:create`, `services.servicios:edit`, `services.servicios:delete`
- `services.categorias:*`
- `services.prendas:*`
- `services.precios:*`
- `services.descuentos:*`
- `users.usuarios:view`, `users.usuarios:create`, `users.usuarios:edit`, `users.usuarios:delete`, `users.usuarios:toggle`
- `users.roles:view`, `users.roles:create`, `users.roles:edit`, `users.roles:delete`

---

## 9. Verificación post-despliegue

### 9.1 Health checks

```bash
# API
curl http://localhost:5000/api/servicios
# Debe retornar JSON (401 si no hay auth)

# Frontend
curl http://localhost/
# Debe servir index.html
```

### 9.2 Logs

```bash
# API .NET
journalctl -u lavanderia-api -f

# Nginx
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### 9.3 Migración de timestamps (verificar)

```bash
# Verificar que la columna usa timestamp without time zone
sudo docker exec -i lavanderia_postgres psql -U lavanderia_user -d lavanderiadb \
  -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'ordenes' AND column_name IN ('fecharecepcion','fechaprometida','fechaentrega');"
```

Debería mostrar `timestamp without time zone` para todas las columnas de fecha.

---

## 10. Actualización (deploy update)

### 10.1 Backend

```bash
# 1. Compilar nueva versión
dotnet publish -c Release -r linux-arm64 --self-contained false -o publish/

# 2. Copiar al servidor
rsync -avz --delete publish/ user@163.192.142.183:/opt/lavanderia/backend/

# 3. Reiniciar servicio
sudo systemctl restart lavanderia-api

# 4. Verificar
sudo systemctl status lavanderia-api
curl http://localhost:5000/api/servicios
```

### 10.2 Frontend

```bash
# 1. Build nuevo
npm run build

# 2. Copiar al servidor
rsync -avz --delete dist/ user@163.192.142.183:/var/www/lavanderia/
```

### 10.3 Migraciones EF Core

Si hay nuevas migraciones, ejecutarlas antes de reiniciar la API:

```bash
# En máquina de desarrollo — generar script SQL
dotnet ef migrations script --startup-project ../LaundryManagement.API -o migration.sql

# Copiar y ejecutar en servidor
sudo docker exec -i lavanderia_postgres psql -U lavanderia_user -d lavanderiadb < migration.sql
```

---

## 11. Datos sensibles — checklist

| Dato | Fuente | Acción |
|------|--------|--------|
| DB password (`lavanderia_user`) | `appsettings.Production.json` o variable de entorno | Nunca commitear, generar aleatorio |
| JWT SecretKey | `JwtSettings__SecretKey` | Mínimo 64 chars, generado con `openssl rand -base64 48` |
| SMTP Gmail Password | `ReportSettings__SmtpPassword` | Gmail App Password — generar en cuenta de Google |
| AllowedOrigins | `appsettings.Production.json` | Solo el dominio/IP de producción |
| AllowedOrigins en frontend | `.env.production` | Coincidir con backend |

---

## 12. Respaldo (backup)

### 12.1 Backup de base de datos

```bash
#!/bin/bash
# backup.sh
DATE=$(date +%Y%m%d_%H%M%S)
FILENAME="lavanderiadb_backup_$DATE.sql"

sudo docker exec lavanderia_postgres pg_dump -U lavanderia_user lavanderiadb > /opt/lavanderia/backups/$FILENAME
gzip /opt/lavanderia/backups/$FILENAME

# Mantener solo los últimos 7 días
find /opt/lavanderia/backups/ -name "lavanderiadb_backup_*.sql.gz" -mtime +7 -delete
```

Agregar al crontab:
```bash
crontab -e
# 0 2 * * * /opt/lavanderia/backup.sh
```

### 12.2 Backup de archivos

```bash
rsync -avz /var/www/lavanderia/ user@backup-server:/var/backups/lavanderia/frontend/
rsync -avz /opt/lavanderia/backend/ user@backup-server:/var/backups/lavanderia/backend/
```

---

## 13. Problemas comunes

### API no levanta

```bash
# Verificar logs
journalctl -u lavanderia-api -n 50

# Verificar conexión a BD
sudo docker exec -it lavanderia_postgres psql -U lavanderia_user -d lavanderiadb -c "SELECT 1;"

# Verificar que el puerto 5000 no esté en uso
sudo lsof -i :5000
```

### Error 502 Bad Gateway

```bash
# La API no está corriendo
sudo systemctl status lavanderia-api
sudo systemctl restart lavanderia-api
```

### Permisos denegados en base de datos

```bash
# Verificar que el usuario tiene permisos
sudo docker exec -it lavanderia_postgres psql -U lavanderia_user -d lavanderiadb -c "\dp"
```

### Frontend no carga (404 en assets)

```bash
# Verificar que los archivos existen
ls -la /var/www/lavanderia/assets/

# Verificar propietario
sudo chown -R www-data:www-data /var/www/lavanderia
```
