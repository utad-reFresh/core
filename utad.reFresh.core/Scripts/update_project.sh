#!/bin/bash
set -e

export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
export PATH=$JAVA_HOME/bin:$PATH
export ANDROID_SDK_ROOT=/opt/android-sdk
export HOME=/home/joaoe

git config --global --add safe.directory /opt/projetos/refresh

# Caminhos
REPO_DIR="/opt/projetos/refresh"
SITE_APK_DIR="/var/www/refresh/wwwroot/apks"
LOG_FILE="/var/log/refresh_build.log"

# Verifica se REPO_DIR existe
if [ ! -d "$REPO_DIR" ]; then
  echo "[ERRO] Diretório $REPO_DIR não encontrado!" >> "$LOG_FILE"
  exit 1
fi

cd "$REPO_DIR" || exit 1


COMMIT_HASH=$(git rev-parse --short HEAD)

# Timestamp
TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
APK_NAME="refresh-${TIMESTAMP}-${COMMIT_HASH}.apk"

# Início do log
echo "=== Build iniciado em $(date) ===" >> "$LOG_FILE"

# Atualiza o repositório
echo "→ Git pull..." >> "$LOG_FILE"
git reset --hard >> "$LOG_FILE" 2>&1
git pull origin main >> "$LOG_FILE" 2>&1

sleep 5

chmod +x gradlew
echo "→ Diretório atual: $(pwd)" >> "$LOG_FILE"
echo "→ Existe gradlew? $(ls -l gradlew)" >> "$LOG_FILE"

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
APK_LATEST="$SITE_APK_DIR/latest.apk"

if [ -f "$APK_ORIGINAL" ]; then
  cp "$APK_ORIGINAL" "$APK_DEST"
  cp "$APK_ORIGINAL" "$APK_LATEST"
  echo "$COMMIT_HASH" > "$SITE_APK_DIR/version.txt"
  echo "✓ APK copiado para: $APK_DEST e $APK_LATEST" >> "$LOG_FILE"
else
  echo "[ERRO] APK não encontrado em $APK_ORIGINAL" >> "$LOG_FILE"
  exit 1
fi


echo "=== Build finalizado com sucesso em $(date) ===" >> "$LOG_FILE"