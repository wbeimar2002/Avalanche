// Auto-generated by the Load Impact converter

import "./libs/shim/core.js";

export let options = { maxRedirects: 4, iterations: "1000" };

const Request = Symbol.for("request");
postman[Symbol.for("initial")]({
  options,
  environment: {
    ADMIN_USR: "admin@admin.com",
    ADMIN_PASS: "12345678"
  }
});

export default function() {
  postman[Request]({
    name: "Health Check",
    id: "719bb5aa-6fff-4c4c-a263-afc073c5e831",
    method: "GET",
    address: "https://192.168.1.60:443/Health/check/",
    headers: {
      authority: "192.168.1.62:443",
      accept: "*/*",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.34 Safari/537.36 Edg/81.0.416.20",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      "sec-fetch-dest": "empty",
      referer: "https://192.168.1.62:443/index.html",
      "accept-language": "en-US,en;q=0.9,es;q=0.8,th;q=0.7"
    },
    post(response) {
      pm.test("Status code is 200", function() {
        pm.response.to.have.status(200);
      });
    }
  });

  postman[Request]({
    name: "Health Check Secure",
    id: "bdb78b20-11da-4302-9a8e-22b6959f1b69",
    method: "GET",
    address: "https://192.168.1.60:443/Health/check/secure",
    headers: {
      authority: "192.168.1.60:443",
      accept: "*/*",
      "sec-fetch-dest": "empty",
      "user-agent":
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36",
      "sec-fetch-site": "same-origin",
      "sec-fetch-mode": "cors",
      referer: "https://192.168.1.60:443/swagger/index.html",
      "accept-language": "en,en-US;q=0.9"
    },
    pre() {
      pm.sendRequest(
        {
          url: "https://192.168.1.62:443/api/login",
          method: "POST",
          header: "Content-Type:application/json",
          body: {
            mode: "application/json",
            raw: JSON.stringify({
              email: pm.environment.get("ADMIN_USR"),
              password: pm.environment.get("ADMIN_PASS")
            })
          }
        },
        function(err, res) {
          console.log(err ? err : res.json());

          if (err === null) {
            console.log("Saving the token");
            var responseJson = res.json();

            console.log("Token: " + responseJson.accessToken);

            pm.environment.set(
              "CURRENT_ACCESS_TOKEN",
              responseJson.accessToken
            );
          }
        }
      );
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
}
