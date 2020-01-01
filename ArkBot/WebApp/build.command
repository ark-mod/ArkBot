ng build --prod --bh /


gh-pages
----------------------------
ng build --demo --base-href "https://tsebring.github.io/ArkBot/"

ngh


ng build --prod --no-aot ; DEL "../bin/x64/Debug/WebApp/" ; COPY "dist\*" "../bin/x64/Debug/WebApp/"