#!/bin/bash

# Caminhos
REPO_DIR="/opt/projetos/refresh"
SITE_APK_DIR="/var/www/refresh/wwwroot/apks"
LOG_FILE="/var/log/refresh_build.log"

# Timestamp
TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
APK_NAME="app-release-$TIMESTAMP.apk"

# Início do log
echo "=== Build iniciado em $(date) ===" >> "$LOG_FILE"

# Verifica se REPO_DIR existe
if [ ! -d "$REPO_DIR" ]; then
  echo "[ERRO] Diretório $REPO_DIR não encontrado!" >> "$LOG_FILE"
  exit 1
fi

# Acessa o diretório
cd "$REPO_DIR" || exit 1

# Atualiza o repositório
echo "→ Git pull..." >> "$LOG_FILE"
git reset --hard >> "$LOG_FILE" 2>&1
git pull origin main >> "$LOG_FILE" 2>&1

# Builda o projeto
echo "→ Buildando APK..." >> "$LOG_FILE"
./gradlew assembleRelease >> "$LOG_FILE" 2>&1
BUILD_STATUS=$?

if [ $BUILD_STATUS -ne 0 ]; then
  echo "[ERRO] Falha ao buildar o projeto." >> "$LOG_FILE"
  exit 1
fi

# Move o APK com nome baseado no timestamp
APK_ORIGINAL="$REPO_DIR/app/build/outputs/apk/release/app-release.apk"
APK_DEST="$SITE_APK_DIR/$APK_NAME"

if [ -f "$APK_ORIGINAL" ]; then
  cp "$APK_ORIGINAL" "$APK_DEST"
  echo "✓ APK copiado para: $APK_DEST" >> "$LOG_FILE"
else
  echo "[ERRO] APK não encontrado em $APK_ORIGINAL" >> "$LOG_FILE"
  exit 1
fi

echo "=== Build finalizado com sucesso em $(date) ===" >> "$LOG_FILE"
