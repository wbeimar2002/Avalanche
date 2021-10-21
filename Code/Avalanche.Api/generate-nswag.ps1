$env:AVALANCHE_CONFIGURATION_SERVICE_HOST="localhost"
$env:AVALANCHE_CONFIGURATION_SERVICE_PORT=80
$env:AVALANCHE_CLIENT_CERTIFICATE_PATH="C:\Olympus\certificates\grpc_localhost_root_l1.pfx"
$env:AVALANCHE_SERVER_VALIDATION_CERTIFICATE_PATH="C:\Olympus\certificates\grpc_localhost_root_l1.cer"
$env:AVALANCHE_CLIENT_CERTIFICATE_PASSWORD="0123456789"
nswag run nswag.json /variables:Configuration=Debug,OutDir=bin\Debug\netcoreapp3.1\ /runtime:NetCore31