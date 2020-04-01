// Auto-generated by the Load Impact converter

import "./libs/shim/core.js";
import "./libs/shim/expect.js";
import http from 'k6/http';
import { check, fail } from 'k6';

export let options = { maxRedirects: 4, iterations: "1" };

const Pre = Symbol.for("pre");
const Request = Symbol.for("request");
postman[Symbol.for("initial")]({
  options,
  environment: {
    ADMIN_USR: "admin@admin.com",
    ADMIN_PASS: "12345678",
    LOGIN_URL: "https://localhost:6001/api/login"
  }
});

export default function() {
  postman[Pre].push(() => {
    
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

  });

  postman[Request]({
    name: "Send Broadcast to RabbitMQ",
    id: "6b2c665b-6a5f-4f91-b85c-425084608df5",
    method: "POST",
    address: "https://localhost:5001/Notifications/queue",
    data: '{"content":"string","eventGroup":0}',
    headers: {
      authority: "localhost:5001",
      accept: "*/*",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "content-type": "application/json",
      origin: "https://localhost:5001",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Successful POST request", function() {
        pm.expect(pm.response.code).to.be.oneOf([201, 202]);
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Request]({
    name: "Send Broadcast Direct to Signal R",
    id: "9ff5484a-e9c8-4ac1-878d-8e2875db5627",
    method: "POST",
    address: "https://localhost:5001/Notifications/direct",
    data: '{"content":"string","eventGroup":0}',
    headers: {
      authority: "localhost:5001",
      accept: "*/*",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "content-type": "application/json",
      origin: "https://localhost:5001",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Successful POST request", function() {
        pm.expect(pm.response.code).to.be.oneOf([201, 202]);
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Pre].pop();
}
