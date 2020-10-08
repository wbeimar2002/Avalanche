To run a collection for integration tests
Reference: https://blog.postman.com/2015/04/09/installing-newman-on-windows/
newman run Avalanche.postman_collection.json -e environment.json --insecure --reporters cli,json --reporter-json-export Avalanche.postman_collection.results.json

To run a collection for load testing
Reference: https://blog.loadimpact.com/load-testing-with-postman-collections

Install Postman to K6
npm install -g postman-to-k6

1. Generate the script based in postman collection
postman-to-k6 Avalanche.postman_collection.json -i 10 --environment environment.json -o Avalanche-k6-script.js 

ADD THIS IMPORTS TO THE SCRIPT HEADER

import http from 'k6/http';
import { check, fail } from 'k6';

pm.sendRequest is not supported for k6 so, this is a simple pre-request

    var currentToken = pm.environment.get("CURRENT_ACCESS_TOKEN");

    if (currentToken === undefined) {

      //https://k6.io/docs/using-k6/http-requests
      var url = pm.environment.get("LOGIN_URL");
      var payload = JSON.stringify({
        email: pm.environment.get("ADMIN_USR"),
        password: pm.environment.get("ADMIN_PASS")
      });

      var params = {
        headers: {
          'Content-Type': 'application/json',
        },
      };

      var res = http.post(url, payload, params);

      // Verify that we ended up on the user page
      check(res, {
        'login succeeded': 'OK'
      }) || fail('login failed');

      //https://k6.io/docs/javascript-api/k6-http/response-k6-http
      //console.log("Saving the token");
      var responseJson = res.json();
      //console.log("Token: " + responseJson.accessToken);

      pm.environment.set(
        "CURRENT_ACCESS_TOKEN",
        responseJson.accessToken
      );
    }


2. Run the script
Download de k6.exe
https://github.com/loadimpact/k6/releases

k6 run Avalanche-k6-script.js --out json=Avalanche.k6.results.json

OPTIONAL > Run the script with docker
References: https://docs.k6.io/docs/running-k6
            https://docs.k6.io/v1.0/docs/modules#section-using-local-modules-with-docker

Notes: Remember change the script to use the right internal port and ip 192.168.1.60:443 (5001) and the secure server 192.168.1.62:443 (6001) and
change this options export let options = { maxRedirects: 4, iterations: "1000", insecureSkipTLSVerify: true };

docker run -v C:\Users\sorey\source\repos\AvalancheApiOfficial\Resources:/src --net app_net --ip 192.168.1.63 -it -i loadimpact/k6 run /src/Health-k6-script.js