To run a collection for integration tests
Reference: https://blog.postman.com/2015/04/09/installing-newman-on-windows/
newman run {collectionname}.postman_collection.json --insecure
newman run Setup.postman_collection.json --insecure

To run a collection for load testing
Reference: https://blog.loadimpact.com/load-testing-with-postman-collections

1. Generate the script based in postman collection
postman-to-k6 {collectionname}.postman_collection.json -i 1000 -o k6-script.js
postman-to-k6 Setup.postman_collection.json -i 1000 -o k6-script.js

2. Run the script
References: https://docs.k6.io/docs/running-k6
            https://docs.k6.io/v1.0/docs/modules#section-using-local-modules-with-docker

Notes: Remember change the script to use the right internal IP and the port 443 for https in our case ip https://192.168.1.63:443
and change this options export let options = { maxRedirects: 4, iterations: "1000", insecureSkipTLSVerify: true };
docker run -v C:\Repos\AvalancheApi\Resources:/src --net app_net --ip 192.168.1.63 -it -i loadimpact/k6 run /src/k6-script.js




