prod build (the one we commit to the repository)
----------------------------
ng build --prod --no-aot --bh /


dev
----------------------------
ng serve --env demo


gh-pages
----------------------------
ng build --env demo --base-href "https://ark-mod.github.io/ArkBot/"

We use angular-cli-ghpages to push it to the repository
Installing angular-cli-ghpages:
npm i angular-cli-ghpages --save-dev

Pushing it:
npx angular-cli-ghpages --repo https://github.com/ark-mod/ArkBot.git





ng build --prod --no-aot ; DEL "../bin/x64/Debug/WebApp/" ; COPY "dist\*" "../bin/x64/Debug/WebApp/"