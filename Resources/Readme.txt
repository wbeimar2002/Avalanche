To run a collection for integration tests
Reference: https://blog.postman.com/2015/04/09/installing-newman-on-windows/
newman run {collectionname}.postman_collection.json -e environment.json --insecure

newman run Health.postman_collection.json -e environment.json --insecure
newman run Licensing.postman_collection.json -e environment.json --insecure
newman run PGS.postman_collection.json -e environment.json --insecure
newman run Security.postman_collection.json -e environment.json --insecure
newman run Setup.postman_collection.json -e environment.json --insecure
newman run StateServer.postman_collection.json -e environment.json --insecure

To run a collection for load testing
Reference: https://blog.loadimpact.com/load-testing-with-postman-collections

1. Generate the script based in postman collection
postman-to-k6 {collectionname}.postman_collection.json -i 1000 -o {collectionname}-k6-script.js

postman-to-k6 Health.postman_collection.json -i 1000 --environment environment.json -o Health-k6-script.js 
postman-to-k6 Licensing.postman_collection.json -i 1000 -o Licensing-k6-script.js
postman-to-k6 PGS.postman_collection.json -i 1000 -o PGS-k6-script.js
postman-to-k6 Security.postman_collection.json -i 1000 -o Security-k6-script.js
postman-to-k6 Setup.postman_collection.json -i 1000 -o Setup-k6-script.js
postman-to-k6 StateServer.postman_collection.json -i 1000 -o StateServer-k6-script.js

2. Run the script
References: https://docs.k6.io/docs/running-k6
            https://docs.k6.io/v1.0/docs/modules#section-using-local-modules-with-docker

Notes: Remember change the script to use the right internal port and ip 192.168.1.60:443 (5001) and the secure server 192.168.1.62:443 (6001)
and change this options export let options = { maxRedirects: 4, iterations: "1000", insecureSkipTLSVerify: true };

docker run -v C:\Users\sorey\source\repos\AvalancheApiOfficial\Resources:/src --net app_net --ip 192.168.1.63 -it -i loadimpact/k6 run /src/Health-k6-script.js

https://github.com/loadimpact/postman-to-k6
npm install -g postman-to-k6
postman-to-k6 collection.json -o k6-script.js
k6 run k6-script.js

ISSUE: Error: pm.sendRequest not supported