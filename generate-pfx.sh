#!/bin/bash

# Script to generate a PKCS#12 PFX file for WeatherDashboard Data Protection
# Requirements:
# - No user input
# - No password
# - RSA 3072
# - 10-year validity
# - Subject CN=WeatherDashboard.Web.DataProtection.Key
# - Output to src/WeatherDashboard.Web/WeatherDashboard.Web.DataProtection.pfx
# - No artifacts left behind

set -e

# Define paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$SCRIPT_DIR/src/WeatherDashboard.Web"
OUTPUT_FILE="$OUTPUT_DIR/WeatherDashboard.Web.DataProtection.pfx"

# Create temporary directory for intermediate files
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

# File paths for temporary artifacts
PRIVATE_KEY="$TEMP_DIR/private.key"
CERT_FILE="$TEMP_DIR/certificate.crt"

echo "Generating RSA 3072 private key and self-signed certificate..."

# Generate private key and self-signed certificate in one command
# -nodes: no DES (no password for private key)
# -newkey rsa:3072: generate new RSA 3072 key
# -x509: output a self-signed certificate
# -days 3650: valid for 10 years
# -subj: subject with CN=WeatherDashboard.Web.DataProtection.Key
# -keyout: output private key
# -out: output certificate
openssl req \
  -nodes \
  -newkey rsa:3072 \
  -x509 \
  -days 3650 \
  -subj "/CN=WeatherDashboard.Web.DataProtection.Key" \
  -keyout "$PRIVATE_KEY" \
  -out "$CERT_FILE" \
  2>/dev/null

echo "Creating PKCS#12 PFX file..."

# Create PKCS#12 file from certificate and private key
# -export: export PKCS#12 file
# -out: output file
# -inkey: input private key
# -in: input certificate
# -passout pass: empty password
openssl pkcs12 \
  -export \
  -out "$OUTPUT_FILE" \
  -inkey "$PRIVATE_KEY" \
  -in "$CERT_FILE" \
  -passout pass: \
  2>/dev/null

echo "Successfully created PFX file at: $OUTPUT_FILE"
echo ""
echo "Certificate details:"
openssl pkcs12 -in "$OUTPUT_FILE" -nokeys -passin pass: 2>/dev/null | \
  openssl x509 -noout -subject -dates
