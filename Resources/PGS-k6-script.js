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
    name: "Get Content Types",
    id: "04fd103e-c9d1-4b93-a9af-1364e621891e",
    method: "GET",
    address: "https://localhost:5001/Metadata/contenttypes",
    headers: {
      authority: "localhost:5001",
      accept: "text/plain",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Status code is 200", function() {
        pm.response.to.have.status(200);
      });

      pm.test("Is an array", function() {
        var jsonData = pm.response.json();
        pm.expect(jsonData).to.be.an("array");
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Request]({
    name: "Get Content by Type",
    id: "6c0d6b19-b782-45b3-8c8a-510bbdefe5b8",
    method: "GET",
    address: "https://localhost:5001/Outputs/Content/P",
    headers: {
      authority: "localhost:5001",
      accept: "text/plain",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Status code is 200", function() {
        pm.response.to.have.status(200);
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Request]({
    name: "Get An State of an Output",
    id: "ce627060-00dd-4fff-a0e8-0efe587e8060",
    method: "GET",
    address: "https://localhost:5001/Outputs/1234/states/1",
    headers: {
      authority: "localhost:5001",
      accept: "text/plain",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Status code is 200", function() {
        pm.response.to.have.status(200);
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Request]({
    name: "Get States of an Output",
    id: "c92d7c22-9843-412f-a0e5-f72aef871833",
    method: "GET",
    address: "https://localhost:5001/Outputs/1234/states",
    headers: {
      authority: "localhost:5001",
      accept: "text/plain",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Status code is 200", function() {
        pm.response.to.have.status(200);
      });

      pm.test("Is an array", function() {
        var jsonData = pm.response.json();
        pm.expect(jsonData).to.be.an("array");
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Request]({
    name: "Get All Outputs",
    id: "cf4ead06-a587-4592-be13-8429ff684c7c",
    method: "GET",
    address: "https://localhost:5001/Outputs/all",
    headers: {
      authority: "localhost:5001",
      accept: "text/plain",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://localhost:5001/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    post(response) {
      pm.test("Status code is 200", function() {
        pm.response.to.have.status(200);
      });

      pm.test("Is an array", function() {
        var jsonData = pm.response.json();
        pm.expect(jsonData).to.be.an("array");
      });
    },
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Request]({
    name: "Save commands to an Ouput",
    id: "07f6303e-7496-454a-a83d-89126d558371",
    method: "PUT",
    address: "https://localhost:5001/Outputs/commands",
    data:
      '{"commandType":0,"value":"string","outputs":[{"id":"string","name":"string","preview":"string","isActive":true}]}',
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
    auth(config, Var) {
      config.headers.Authorization = `Bearer ${pm[Var](
        "CURRENT_ACCESS_TOKEN"
      )}`;
    }
  });

  postman[Pre].pop();
}
