webpackJsonp([1,5],{

/***/ 10:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__ = __webpack_require__(35);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__message_service__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__environments_environment__ = __webpack_require__(22);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5_moment__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5_moment___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_5_moment__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DataService; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};






var DataService = (function () {
    function DataService(httpService, messageService) {
        var _this = this;
        this.httpService = httpService;
        this.messageService = messageService;
        this._servers = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
        this.menuOption = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
        this.theme = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
        this.ServersUpdated$ = new __WEBPACK_IMPORTED_MODULE_0__angular_core__["t" /* EventEmitter */]();
        messageService.serverUpdated$.subscribe(function (serverKey) { return _this.updateServer(serverKey); });
        //this.getServers();
    }
    Object.defineProperty(DataService.prototype, "Theme", {
        get: function () {
            return this.theme.asObservable();
        },
        enumerable: true,
        configurable: true
    });
    DataService.prototype.SetTheme = function (theme) {
        this.theme.next(theme);
    };
    Object.defineProperty(DataService.prototype, "MenuOption", {
        get: function () {
            return this.menuOption.asObservable();
        },
        enumerable: true,
        configurable: true
    });
    DataService.prototype.SetMenuOption = function (menuOption) {
        this.menuOption.next(menuOption);
    };
    DataService.prototype.getServers = function () {
        var _this = this;
        return this.httpService
            .getServers()
            .then(function (servers) {
            _this.Servers = servers;
            var user = servers ? servers.User : undefined;
            _this.UserSteamId = user && user.SteamId ? user.SteamId : undefined;
            _this._servers.next(servers);
            _this.ServersUpdated$.emit(servers);
            return true;
        })
            .catch(function (error) {
            _this.Servers = null;
            _this.UserSteamId = undefined;
            _this._servers.next(null);
            _this.ServersUpdated$.emit(null);
            return false;
        });
    };
    DataService.prototype.updateServer = function (serverKey) {
        this.getServers();
    };
    DataService.prototype.hasFeatureAccess = function (featureGroup, featureName, forSteamId) {
        var accessControl = this.Servers ? this.Servers.AccessControl : undefined;
        if (!accessControl)
            return false;
        var fg = accessControl[featureGroup];
        if (!fg)
            return false;
        var rf = fg[featureName];
        if (!rf)
            return false;
        var user = this.Servers ? this.Servers.User : undefined;
        var userRoles = (user && user.Roles ? user.Roles.slice(0) : []);
        if (user && user.SteamId && user.SteamId == forSteamId)
            userRoles.push("self");
        var _loop_1 = function (urole) {
            if (rf.find(function (value) { return urole.toLowerCase() === value.toLowerCase(); }))
                return { value: true };
        };
        for (var _i = 0, userRoles_1 = userRoles; _i < userRoles_1.length; _i++) {
            var urole = userRoles_1[_i];
            var state_1 = _loop_1(urole);
            if (typeof state_1 === "object")
                return state_1.value;
        }
        return false;
    };
    DataService.prototype.hasFeatureAccessObservable = function (featureGroup, featureName, forSteamId) {
        var _this = this;
        return this._servers.asObservable().map(function (v) {
            var foo = _this.hasFeatureAccess(featureGroup, featureName, forSteamId);
            return foo;
        });
    };
    DataService.prototype.getCurrentDate = function () {
        return !__WEBPACK_IMPORTED_MODULE_4__environments_environment__["a" /* environment */].demo ? __WEBPACK_IMPORTED_MODULE_5_moment__() : __WEBPACK_IMPORTED_MODULE_5_moment__(new Date(__WEBPACK_IMPORTED_MODULE_4__environments_environment__["a" /* environment */].demoDate));
    };
    DataService.prototype.toDate = function (datejson) {
        //todo: fix locale
        return new Date(datejson).toLocaleString('sv-SE');
    };
    DataService.prototype.toRelativeDate = function (datejson) {
        if (!datejson)
            return "";
        if (!__WEBPACK_IMPORTED_MODULE_4__environments_environment__["a" /* environment */].demo)
            return __WEBPACK_IMPORTED_MODULE_5_moment__(new Date(datejson)).fromNow();
        else
            return __WEBPACK_IMPORTED_MODULE_5_moment__(new Date(datejson)).from(new Date(__WEBPACK_IMPORTED_MODULE_4__environments_environment__["a" /* environment */].demoDate));
    };
    return DataService;
}());
DataService = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["d" /* Injectable */])(),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_2__http_service__["a" /* HttpService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__http_service__["a" /* HttpService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */]) === "function" && _b || Object])
], DataService);

var _a, _b;
//# sourceMappingURL=data.service.js.map

/***/ }),

/***/ 105:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "c", function() { return DataTableColumnCellDirective; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "b", function() { return DataTableColumnHeaderDirective; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DataTableColumnDirective; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};

var DataTableColumnCellDirective = (function () {
    function DataTableColumnCellDirective(template) {
        this.template = template;
    }
    return DataTableColumnCellDirective;
}());
DataTableColumnCellDirective = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["k" /* Directive */])({ selector: '[ark-dt-cell]' }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */]) === "function" && _a || Object])
], DataTableColumnCellDirective);

var DataTableColumnHeaderDirective = (function () {
    function DataTableColumnHeaderDirective(template) {
        this.template = template;
    }
    return DataTableColumnHeaderDirective;
}());
DataTableColumnHeaderDirective = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["k" /* Directive */])({ selector: '[ark-dt-header]' }),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */]) === "function" && _b || Object])
], DataTableColumnHeaderDirective);

var DataTableColumnDirective = (function () {
    function DataTableColumnDirective() {
    }
    return DataTableColumnDirective;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_10" /* ContentChild */])(DataTableColumnCellDirective, { read: __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */] }),
    __metadata("design:type", typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */]) === "function" && _c || Object)
], DataTableColumnDirective.prototype, "cellTemplate", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_10" /* ContentChild */])(DataTableColumnHeaderDirective, { read: __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */] }),
    __metadata("design:type", typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["P" /* TemplateRef */]) === "function" && _d || Object)
], DataTableColumnDirective.prototype, "headerTemplate", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableColumnDirective.prototype, "mode", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableColumnDirective.prototype, "key", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableColumnDirective.prototype, "thenSort", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableColumnDirective.prototype, "title", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Boolean)
], DataTableColumnDirective.prototype, "orderBy", void 0);
DataTableColumnDirective = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["k" /* Directive */])({ selector: 'ark-dt-column' })
], DataTableColumnDirective);

var _a, _b, _c, _d;
//# sourceMappingURL=column.directive.js.map

/***/ }),

/***/ 106:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__ = __webpack_require__(35);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DataTableModeDirective; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var DataTableModeDirective = (function () {
    function DataTableModeDirective() {
        this.enabled = __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["Observable"].of(true);
        /*@Input()
        set enabled(val) {
            this._enabled.next(val);
        };
    
        get enabled() {
            return this._enabled.getValue();
        }*/
    }
    Object.defineProperty(DataTableModeDirective.prototype, "columnKeys", {
        set: function (val) {
            this._columnKeys = val;
            this.ColumnKeys = this._columnKeys.split(',');
        },
        enumerable: true,
        configurable: true
    });
    ;
    return DataTableModeDirective;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableModeDirective.prototype, "key", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableModeDirective.prototype, "name", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["Observable"] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["Observable"]) === "function" && _a || Object)
], DataTableModeDirective.prototype, "enabled", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String),
    __metadata("design:paramtypes", [String])
], DataTableModeDirective.prototype, "columnKeys", null);
DataTableModeDirective = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["k" /* Directive */])({ selector: 'ark-dt-mode' })
], DataTableModeDirective);

var _a;
//# sourceMappingURL=mode.directive.js.map

/***/ }),

/***/ 107:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (immutable) */ __webpack_exports__["c"] = floatCompare;
/* harmony export (immutable) */ __webpack_exports__["b"] = intCompare;
/* harmony export (immutable) */ __webpack_exports__["a"] = stringLocaleCompare;
/* unused harmony export nullCompare */
// ----------------------------------------------------
// Compare functions for sorting
// ----------------------------------------------------
// ----------------------------------------------------
function floatCompare(v1, v2, asc, decimals) {
    var nullCheck = nullCompare(v1, v2, asc);
    if (nullCheck != undefined)
        return nullCheck;
    var base = Math.pow(10, decimals);
    var f1 = decimals != undefined ? Math.round(v1 * base) / base : v1;
    var f2 = decimals != undefined ? Math.round(v2 * base) / base : v2;
    return f1 > f2 ? (asc ? 1 : -1) : f1 < f2 ? (asc ? -1 : 1) : 0;
}
function intCompare(v1, v2, asc) {
    var nullCheck = nullCompare(v1, v2, asc);
    if (nullCheck != undefined)
        return nullCheck;
    return v1 > v2 ? (asc ? 1 : -1) : v1 < v2 ? (asc ? -1 : 1) : 0;
}
function stringLocaleCompare(v1, v2, asc) {
    var nullCheck = nullCompare(v1, v2, asc);
    if (nullCheck != undefined)
        return nullCheck;
    var r = v1.localeCompare(v2);
    return asc ? r : (r == 1 ? -1 : r == -1 ? 1 : 0);
}
function nullCompare(v1, v2, asc) {
    if (v1 == null && v2 == null)
        return 0;
    else if (v1 == null)
        return 1; //always below
    else if (v2 == null)
        return -1; //always below
    return undefined;
}
// ----------------------------------------------------
// Assorted functions
// ---------------------------------------------------- 
//# sourceMappingURL=utils.js.map

/***/ }),

/***/ 19:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__environments_environment__ = __webpack_require__(22);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return MessageService; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var MessageService = (function () {
    function MessageService(zone) {
        this.zone = zone;
        this.serverUpdated$ = new __WEBPACK_IMPORTED_MODULE_0__angular_core__["t" /* EventEmitter */]();
    }
    MessageService.prototype.connect = function () {
        var _this = this;
        this.connection = $.hubConnection(this.getSignalRBaseUrl());
        this.proxy = this.connection.createHubProxy('ServerUpdateHub');
        this.proxy.on('serverUpdateNotification', function (serverKey) {
            _this.zone.run(function () {
                _this.serverUpdated$.emit(serverKey);
            });
        });
        this.connection.start()
            .done(function () { return console.log('Now connected, connection ID=' + _this.connection.id); })
            .fail(function () { return console.log('Could not connect'); });
    };
    MessageService.prototype.getSignalRBaseUrl = function () {
        return __WEBPACK_IMPORTED_MODULE_1__environments_environment__["a" /* environment */].signalrBaseUrl
            .replace(/\<protocol\>/gi, window.location.protocol)
            .replace(/\<hostname\>/gi, window.location.hostname)
            .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    };
    return MessageService;
}());
MessageService = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["d" /* Injectable */])(),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["V" /* NgZone */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["V" /* NgZone */]) === "function" && _a || Object])
], MessageService);

var _a;
//# sourceMappingURL=message.service.js.map

/***/ }),

/***/ 22:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__environment_common__ = __webpack_require__(320);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return environment; });

var environment = {
    production: true,
    demo: true,
    demoDate: '2017-11-10T16:30:00.0000000Z',
    configJsOverride: __WEBPACK_IMPORTED_MODULE_0__environment_common__["a" /* commonEnvironment */].configJs,
    apiBaseUrl: null,
    signalrBaseUrl: null
};
//# sourceMappingURL=environment.js.map

/***/ }),

/***/ 283:
/***/ (function(module, exports) {

function webpackEmptyContext(req) {
	throw new Error("Cannot find module '" + req + "'.");
}
webpackEmptyContext.keys = function() { return []; };
webpackEmptyContext.resolve = webpackEmptyContext;
module.exports = webpackEmptyContext;
webpackEmptyContext.id = 283;


/***/ }),

/***/ 284:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser_dynamic__ = __webpack_require__(291);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__app_app_module__ = __webpack_require__(299);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__environments_environment__ = __webpack_require__(22);




if (__WEBPACK_IMPORTED_MODULE_3__environments_environment__["a" /* environment */].production) {
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["a" /* enableProdMode */])();
}
__webpack_require__.i(__WEBPACK_IMPORTED_MODULE_1__angular_platform_browser_dynamic__["a" /* platformBrowserDynamic */])().bootstrapModule(__WEBPACK_IMPORTED_MODULE_2__app_app_module__["a" /* AppModule */]);
//# sourceMappingURL=main.js.map

/***/ }),

/***/ 29:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_http__ = __webpack_require__(68);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_rxjs_add_operator_toPromise__ = __webpack_require__(93);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_rxjs_add_operator_toPromise___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_rxjs_add_operator_toPromise__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__environments_environment__ = __webpack_require__(22);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return HttpService; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var HttpService = (function () {
    function HttpService(http) {
        this.http = http;
        this.headers = new __WEBPACK_IMPORTED_MODULE_1__angular_http__["c" /* Headers */]({ 'Content-Type': 'application/json' });
        this.serversUrl = '/servers';
        this.serverUrl = '/server';
        this.wildCreaturesUrl = '/wildcreatures';
        this.structuresUrl = '/structures';
        this.adminServerUrl = '/adminserver';
        this.administerUrl = '/administer';
        this.playerUrl = '/player';
    }
    HttpService.prototype.getOptions = function () {
        var demoMode = localStorage.getItem('demoMode') == 'true';
        var options = new __WEBPACK_IMPORTED_MODULE_1__angular_http__["d" /* RequestOptions */]({ withCredentials: true });
        if (demoMode) {
            if (!options.headers)
                options.headers = new __WEBPACK_IMPORTED_MODULE_1__angular_http__["c" /* Headers */]();
            options.headers.append("demoMode", "true");
        }
        return options;
    };
    HttpService.prototype.getServers = function () {
        return this.http.get("" + this.getApiBaseUrl() + this.serversUrl + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.getServer = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.serverUrl + "/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.getWildCreatures = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.wildCreaturesUrl + "/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.getStructures = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.structuresUrl + "/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.getPlayer = function (steamId) {
        return this.http.get("" + this.getApiBaseUrl() + this.playerUrl + "/" + steamId + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.getAdminServer = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.adminServerUrl + "/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminDestroyAllStructuresForTeamId = function (serverKey, teamId) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/DestroyAllStructuresForTeamId/" + serverKey + "?teamId=" + teamId + "&t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminDestroyStructuresForTeamIdAtPosition = function (serverKey, teamId, x, y, radius, rafts) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/DestroyStructuresForTeamIdAtPosition/" + serverKey + "?teamId=" + teamId + "&x=" + x + "&y=" + y + "&radius=" + radius + "&rafts=" + rafts + "&t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminDestroyDinosForTeamId = function (serverKey, teamId) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/DestroyDinosForTeamId/" + serverKey + "?teamId=" + teamId + "&t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminSaveWorld = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/SaveWorld/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminListFertilizedEggs = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/DroppedEggsList/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminDestroyAllEggs = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/DestroyAllEggs/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.adminDestroySpoiledEggs = function (serverKey) {
        return this.http.get("" + this.getApiBaseUrl() + this.administerUrl + "/DestroySpoiledEggs/" + serverKey + "?t=" + +new Date(), this.getOptions())
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    HttpService.prototype.getApiBaseUrl = function () {
        return __WEBPACK_IMPORTED_MODULE_3__environments_environment__["a" /* environment */].apiBaseUrl
            .replace(/\<protocol\>/gi, window.location.protocol)
            .replace(/\<hostname\>/gi, window.location.hostname)
            .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    };
    HttpService.prototype.handleError = function (error) {
        return Promise.reject(error.message || error);
    };
    return HttpService;
}());
HttpService = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["d" /* Injectable */])(),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__angular_http__["b" /* Http */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_http__["b" /* Http */]) === "function" && _a || Object])
], HttpService);

var _a;
//# sourceMappingURL=http.service.js.map

/***/ }),

/***/ 294:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_rxjs_Observable__ = __webpack_require__(0);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_rxjs_Observable___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_rxjs_Observable__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AccessControlRouteGuardService; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var AccessControlRouteGuardService = (function () {
    function AccessControlRouteGuardService(dataService, router) {
        this.dataService = dataService;
        this.router = router;
    }
    AccessControlRouteGuardService.prototype.canActivate = function (route, state) {
        var _this = this;
        return __WEBPACK_IMPORTED_MODULE_2_rxjs_Observable__["Observable"].fromPromise(this.dataService.getServers().then(function (e) {
            if (e) {
                var pid = route.params['playerid'];
                return _this.dataService.hasFeatureAccess("pages", route.data.name, pid) ? "access" : "noaccess";
            }
            return "connectionerror";
        }).catch(function () {
            return "connectionerror";
        })).map(function (e) {
            if (e == "noaccess")
                _this.router.navigateByUrl('/accessdenied', { skipLocationChange: true });
            else if (e == "connectionerror")
                _this.router.navigateByUrl('/connectionerror', { skipLocationChange: true });
            return e == "access";
        });
    };
    return AccessControlRouteGuardService;
}());
AccessControlRouteGuardService = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["d" /* Injectable */])(),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */]) === "function" && _b || Object])
], AccessControlRouteGuardService);

var _a, _b;
//# sourceMappingURL=access-control-route-guard.service.js.map

/***/ }),

/***/ 295:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__message_service__ = __webpack_require__(19);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AccessDeniedComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var AccessDeniedComponent = (function () {
    function AccessDeniedComponent(dataService, messageService, notificationsService) {
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.menuOption = undefined;
    }
    AccessDeniedComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) { return _this.showServerUpdateNotification(serverKey); });
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) { return _this.menuOption = menuOption; });
    };
    AccessDeniedComponent.prototype.ngOnDestroy = function () {
        this.serverUpdatedSubscription.unsubscribe();
        this.menuOptionSubscription.unsubscribe();
    };
    AccessDeniedComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    AccessDeniedComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    return AccessDeniedComponent;
}());
AccessDeniedComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-access-denied',
        template: __webpack_require__(398),
        styles: [__webpack_require__(376)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _c || Object])
], AccessDeniedComponent);

var _a, _b, _c;
//# sourceMappingURL=access-denied.component.js.map

/***/ }),

/***/ 296:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__ = __webpack_require__(39);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AdminServerMenuComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};



var AdminServerMenuComponent = (function () {
    function AdminServerMenuComponent(dataService) {
        this.dataService = dataService;
    }
    AdminServerMenuComponent.prototype.ngOnInit = function () {
        if (this.dataService.hasFeatureAccess('admin-server', 'structures'))
            this.menu.activate("structures");
        else if (this.dataService.hasFeatureAccess('admin-server', 'players'))
            this.menu.activate("players");
        else if (this.dataService.hasFeatureAccess('admin-server', 'tribes'))
            this.menu.activate("tribes");
        else if (this.dataService.hasFeatureAccess('admin-server', 'eggs'))
            this.menu.activate("eggs");
    };
    return AdminServerMenuComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('menu'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */]) === "function" && _a || Object)
], AdminServerMenuComponent.prototype, "menu", void 0);
AdminServerMenuComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-admin-server-menu',
        host: { '[class]': 'menu.className' },
        template: __webpack_require__(399),
        styles: [__webpack_require__(377)]
    }),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */]) === "function" && _b || Object])
], AdminServerMenuComponent);

var _a, _b;
//# sourceMappingURL=admin-server-menu.component.js.map

/***/ }),

/***/ 297:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__message_service__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_d3__ = __webpack_require__(88);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_d3___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_6_d3__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AdminServerComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







var AdminServerComponent = (function () {
    function AdminServerComponent(route, router, httpService, dataService, messageService, notificationsService) {
        this.route = route;
        this.router = router;
        this.httpService = httpService;
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.menuOption = undefined;
        this.loaded = false;
        this.loadedStructures = false;
        this.loadedFertilizedEggs = false;
    }
    AdminServerComponent.prototype.getServer = function () {
        var _this = this;
        this.httpService
            .getAdminServer(this.serverKey)
            .then(function (server) {
            _this.server = server;
            _this.loaded = true;
        })
            .catch(function (error) {
            _this.server = null;
            _this.loaded = true;
        });
    };
    AdminServerComponent.prototype.getStructures = function () {
        var _this = this;
        this.httpService
            .getStructures(this.serverKey)
            .then(function (structures) {
            _this.structures = structures;
            _this.loadedStructures = true;
        })
            .catch(function (error) {
            _this.structures = undefined;
            _this.loadedStructures = true;
        });
    };
    AdminServerComponent.prototype.getListFertilizedEggs = function () {
        var _this = this;
        this.httpService
            .adminListFertilizedEggs(this.serverKey)
            .then(function (fertilizedEggs) {
            _this.spoiledEggsList = fertilizedEggs.SpoiledEggList;
            _this.fertilizedEggsList = fertilizedEggs.FertilizedEggList;
            _this.fertilizedEggsCount = fertilizedEggs.FertilizedEggsCount === undefined ? 0 : fertilizedEggs.FertilizedEggsCount;
            _this.spoiledEggsCount = fertilizedEggs.SpoiledEggsCount === undefined ? 0 : fertilizedEggs.SpoiledEggsCount;
            _this.totalEggCount = _this.spoiledEggsCount + _this.fertilizedEggsCount;
            _this.loadedFertilizedEggs = true;
        })
            .catch(function (error) {
            _this.fertilizedEggsList = undefined;
            _this.loadedFertilizedEggs = true;
        });
    };
    AdminServerComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.serverKey = this.route.snapshot.params['id'];
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) {
            _this.menuOption = menuOption;
            if (_this.menuOption == "structures") {
                _this.getStructures();
            }
            else if (_this.menuOption == "fertilized-eggs") {
                _this.getListFertilizedEggs();
            }
        });
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) {
            if (_this.serverKey == serverKey) {
                _this.updateServer();
                _this.showServerUpdateNotification(serverKey);
            }
        });
        this.getServer();
    };
    AdminServerComponent.prototype.ngOnDestroy = function () {
        this.menuOptionSubscription.unsubscribe();
        this.serverUpdatedSubscription.unsubscribe();
    };
    AdminServerComponent.prototype.getTribeMember = function (steamId) {
        return this.server.Players.find(function (p) { return p.SteamId == steamId; });
    };
    AdminServerComponent.prototype.updateServer = function () {
        this.getServer();
    };
    AdminServerComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    AdminServerComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    AdminServerComponent.prototype.showInfoModal = function (header, message) {
        var modalInfo = {};
        modalInfo.Header = header;
        modalInfo.Message = message;
        this.modalInfo = modalInfo;
        var cm = __WEBPACK_IMPORTED_MODULE_6_d3__["select"](this.contextMenu.nativeElement);
        cm.style("display", "block");
        if (__WEBPACK_IMPORTED_MODULE_6_d3__["event"])
            __WEBPACK_IMPORTED_MODULE_6_d3__["event"].stopPropagation();
    };
    AdminServerComponent.prototype.hideContextMenu = function () {
        var cm = __WEBPACK_IMPORTED_MODULE_6_d3__["select"](this.contextMenu.nativeElement);
        cm.style("display", "none");
        this.modalInfo = undefined;
    };
    AdminServerComponent.prototype.saveWorld = function (event) {
        var _this = this;
        this.httpService.adminSaveWorld(this.serverKey)
            .then(function (response) {
            _this.hideContextMenu();
            _this.getListFertilizedEggs();
            _this.showInfoModal("Action Successfull!", response.Message);
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    AdminServerComponent.prototype.destroyAllEggs = function (event) {
        var _this = this;
        this.httpService.adminDestroyAllEggs(this.serverKey)
            .then(function (response) {
            _this.hideContextMenu();
            _this.getListFertilizedEggs();
            _this.showInfoModal("Action Successfull!", response.Message);
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    AdminServerComponent.prototype.destroySpoiledEggs = function (event) {
        var _this = this;
        this.httpService.adminDestroySpoiledEggs(this.serverKey)
            .then(function (response) {
            _this.hideContextMenu();
            _this.getListFertilizedEggs();
            _this.showInfoModal("Action Successfull!", response.Message);
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    return AdminServerComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('contextMenu'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */]) === "function" && _a || Object)
], AdminServerComponent.prototype, "contextMenu", void 0);
AdminServerComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-admin-server',
        template: __webpack_require__(400),
        styles: [__webpack_require__(378)]
    }),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["g" /* ActivatedRoute */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["g" /* ActivatedRoute */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */]) === "function" && _c || Object, typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_5__http_service__["a" /* HttpService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_5__http_service__["a" /* HttpService */]) === "function" && _d || Object, typeof (_e = typeof __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */]) === "function" && _e || Object, typeof (_f = typeof __WEBPACK_IMPORTED_MODULE_4__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_4__message_service__["a" /* MessageService */]) === "function" && _f || Object, typeof (_g = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _g || Object])
], AdminServerComponent);

var _a, _b, _c, _d, _e, _f, _g;
//# sourceMappingURL=admin-server.component.js.map

/***/ }),

/***/ 298:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_ng2_breadcrumb_ng2_breadcrumb__ = __webpack_require__(104);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__message_service__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7__environments_environment__ = __webpack_require__(22);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8__angular_platform_browser__ = __webpack_require__(18);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AppComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};









var AppComponent = (function () {
    function AppComponent(doc, messageService, dataService, httpService, breadcrumbService, notificationsService, router) {
        this.doc = doc;
        this.messageService = messageService;
        this.dataService = dataService;
        this.httpService = httpService;
        this.breadcrumbService = breadcrumbService;
        this.notificationsService = notificationsService;
        this.router = router;
        this.notificationOptions = {
            position: ["top", "right"],
            timeOut: 1000,
            lastOnBottom: false
        };
        this.showLogin = false;
        this.currentUrl = "/";
        this.serversUpdatedBefore = false;
        this.loading = true;
        var s = this.doc.createElement('script');
        s.type = 'text/javascript';
        if (__WEBPACK_IMPORTED_MODULE_7__environments_environment__["a" /* environment */].configJsOverride == null)
            s.src = "config.js";
        else
            s.innerHTML = __WEBPACK_IMPORTED_MODULE_7__environments_environment__["a" /* environment */].configJsOverride;
        var head = this.doc.getElementsByTagName('head')[0];
        head.appendChild(s);
        breadcrumbService.addFriendlyNameForRoute('/accessdenied', 'Access Denied');
        breadcrumbService.addFriendlyNameForRoute('/connectionerror', 'Connection error');
        breadcrumbService.hideRoute('/player');
        breadcrumbService.hideRoute('/servers');
        breadcrumbService.hideRoute('/server');
        breadcrumbService.hideRoute('/admin');
        breadcrumbService.addCallbackForRouteRegex('^/player/.+$', this.getNameForPlayer);
        if (!__WEBPACK_IMPORTED_MODULE_7__environments_environment__["a" /* environment */].demo)
            messageService.connect();
    }
    AppComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.dataService.SetTheme(this.getTheme());
        this.routerEventsSubscription = this.router.events.subscribe(function (event) {
            _this.navigationInterceptor(event);
        });
        this.currentUrl = window.location.href || '/';
        this.serversUpdatedSubscription = this.dataService.ServersUpdated$.subscribe(function (servers) {
            if (!_this.serversUpdatedBefore) {
                if (servers && (!servers.User || !servers.User.SteamId)) {
                    //prompt for login
                    _this.showLogin = true;
                }
            }
            _this.serversUpdatedBefore = true;
        });
    };
    AppComponent.prototype.ngOnDestroy = function () {
        this.routerEventsSubscription.unsubscribe();
        this.serversUpdatedSubscription.unsubscribe();
    };
    AppComponent.prototype.navigationInterceptor = function (event) {
        if (event instanceof __WEBPACK_IMPORTED_MODULE_1__angular_router__["d" /* NavigationStart */])
            this.loading = true;
        else if (event instanceof __WEBPACK_IMPORTED_MODULE_1__angular_router__["c" /* NavigationEnd */])
            this.loading = false;
        else if (event instanceof __WEBPACK_IMPORTED_MODULE_1__angular_router__["e" /* NavigationCancel */])
            this.loading = false;
        else if (event instanceof __WEBPACK_IMPORTED_MODULE_1__angular_router__["f" /* NavigationError */])
            this.loading = false;
    };
    AppComponent.prototype.getNameForPlayer = function (id) {
        return "Player";
    };
    AppComponent.prototype.getDefaultTheme = function () {
        var value = (typeof config !== 'undefined' && config.webapp !== 'undefined' && typeof config.webapp.defaultTheme === 'string' ? config.webapp.defaultTheme.toLowerCase() : undefined);
        return value != 'light' && value != 'dark' ? 'dark' : value;
    };
    AppComponent.prototype.getTheme = function () {
        return localStorage.getItem('theme') || this.getDefaultTheme();
    };
    AppComponent.prototype.setTheme = function (theme) {
        this.dataService.SetTheme(theme);
        localStorage.setItem('theme', theme);
        return false;
    };
    AppComponent.prototype.openLogin = function (event) {
        this.showLogin = true;
        event.stopPropagation();
        event.preventDefault();
    };
    AppComponent.prototype.closeLogin = function (event) {
        this.showLogin = false;
    };
    AppComponent.prototype.getLoginUrl = function () {
        return !__WEBPACK_IMPORTED_MODULE_7__environments_environment__["a" /* environment */].demo ? this.httpService.getApiBaseUrl() + '/authentication/login' : '';
    };
    AppComponent.prototype.getLogoutUrl = function () {
        return !__WEBPACK_IMPORTED_MODULE_7__environments_environment__["a" /* environment */].demo ? this.httpService.getApiBaseUrl() + '/authentication/logout?returnUrl=' + this.currentUrl : '';
    };
    return AppComponent;
}());
AppComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'body',
        host: { '[class]': 'getTheme()' },
        template: __webpack_require__(401),
        styles: [__webpack_require__(379)]
    }),
    __param(0, __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["G" /* Inject */])(__WEBPACK_IMPORTED_MODULE_8__angular_platform_browser__["e" /* DOCUMENT */])),
    __metadata("design:paramtypes", [Object, typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_4__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_4__message_service__["a" /* MessageService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_5__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_5__data_service__["a" /* DataService */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_6__http_service__["a" /* HttpService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_6__http_service__["a" /* HttpService */]) === "function" && _c || Object, typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_3_ng2_breadcrumb_ng2_breadcrumb__["b" /* BreadcrumbService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3_ng2_breadcrumb_ng2_breadcrumb__["b" /* BreadcrumbService */]) === "function" && _d || Object, typeof (_e = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _e || Object, typeof (_f = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */]) === "function" && _f || Object])
], AppComponent);

var _a, _b, _c, _d, _e, _f;
//# sourceMappingURL=app.component.js.map

/***/ }),

/***/ 299:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_platform_browser__ = __webpack_require__(18);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser_animations__ = __webpack_require__(292);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__angular_forms__ = __webpack_require__(103);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__angular_http__ = __webpack_require__(68);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7_ng2_breadcrumb_ng2_breadcrumb__ = __webpack_require__(104);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8__app_component__ = __webpack_require__(298);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_9__player_player_component__ = __webpack_require__(311);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_10__player_menu_player_menu_component__ = __webpack_require__(310);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_11__server_server_component__ = __webpack_require__(318);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_12__server_list_server_list_component__ = __webpack_require__(316);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_13__admin_server_admin_server_component__ = __webpack_require__(297);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_14__arkmap_component__ = __webpack_require__(301);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_15__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_16__demo_http_service__ = __webpack_require__(308);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_17__message_service__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_18__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_19__data_resolver_service__ = __webpack_require__(305);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_20__access_control_route_guard_service__ = __webpack_require__(294);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_21__sanitize_style_pipe__ = __webpack_require__(314);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_22__sanitize_html_pipe__ = __webpack_require__(313);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_23__clickOutside_directive__ = __webpack_require__(302);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_24__server_list_menu_server_list_menu_component__ = __webpack_require__(315);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_25__menu_menu_component__ = __webpack_require__(39);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_26__server_menu_server_menu_component__ = __webpack_require__(317);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_27__admin_server_menu_admin_server_menu_component__ = __webpack_require__(296);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_28__arkmap_structures_arkmap_structures_component__ = __webpack_require__(300);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_29__timer_timer_component__ = __webpack_require__(319);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_30__relative_time_relative_time_component__ = __webpack_require__(312);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_31__confirm_button_confirm_button_component__ = __webpack_require__(303);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_32__access_denied_access_denied_component__ = __webpack_require__(295);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_33__connection_error_connection_error_component__ = __webpack_require__(304);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_34__developer_developer_component__ = __webpack_require__(309);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_35__data_table_data_table_module__ = __webpack_require__(307);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_36__environments_environment__ = __webpack_require__(22);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return AppModule; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};





































var appRoutes = [
    {
        path: 'player/:playerid',
        canActivate: [__WEBPACK_IMPORTED_MODULE_20__access_control_route_guard_service__["a" /* AccessControlRouteGuardService */]],
        //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
        data: { name: 'player' },
        children: [
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_9__player_player_component__["a" /* PlayerComponent */]
            },
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_10__player_menu_player_menu_component__["a" /* PlayerMenuComponent */],
                outlet: 'menu'
            }
        ]
    },
    {
        path: 'server/:id',
        canActivate: [__WEBPACK_IMPORTED_MODULE_20__access_control_route_guard_service__["a" /* AccessControlRouteGuardService */]],
        //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
        data: { name: 'server' },
        children: [
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_11__server_server_component__["a" /* ServerComponent */]
            },
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_26__server_menu_server_menu_component__["a" /* ServerMenuComponent */],
                outlet: 'menu'
            }
        ]
    },
    {
        path: 'admin/:id',
        canActivate: [__WEBPACK_IMPORTED_MODULE_20__access_control_route_guard_service__["a" /* AccessControlRouteGuardService */]],
        //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
        data: { name: 'admin-server' },
        children: [
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_13__admin_server_admin_server_component__["a" /* AdminServerComponent */]
            },
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_27__admin_server_menu_admin_server_menu_component__["a" /* AdminServerMenuComponent */],
                outlet: 'menu'
            }
        ]
    },
    {
        path: 'servers',
        canActivate: [__WEBPACK_IMPORTED_MODULE_20__access_control_route_guard_service__["a" /* AccessControlRouteGuardService */]],
        //resolve: { dataService: DataServiceResolver }, //only use when testing without canActivate (it does the same thing)
        data: { name: 'home' },
        children: [
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_12__server_list_server_list_component__["a" /* ServerListComponent */]
            },
            {
                path: '',
                component: __WEBPACK_IMPORTED_MODULE_24__server_list_menu_server_list_menu_component__["a" /* ServerListMenuComponent */],
                outlet: 'menu'
            }
        ]
    },
    {
        path: 'developer',
        component: __WEBPACK_IMPORTED_MODULE_34__developer_developer_component__["a" /* DeveloperComponent */]
    },
    {
        path: 'accessdenied',
        component: __WEBPACK_IMPORTED_MODULE_32__access_denied_access_denied_component__["a" /* AccessDeniedComponent */]
    },
    {
        path: 'connectionerror',
        component: __WEBPACK_IMPORTED_MODULE_33__connection_error_connection_error_component__["a" /* ConnectionErrorComponent */]
    },
    { path: '',
        redirectTo: '/servers',
        pathMatch: 'full'
    }
];
var AppModule = (function () {
    function AppModule() {
    }
    return AppModule;
}());
AppModule = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_2__angular_core__["b" /* NgModule */])({
        declarations: [
            __WEBPACK_IMPORTED_MODULE_8__app_component__["a" /* AppComponent */],
            __WEBPACK_IMPORTED_MODULE_12__server_list_server_list_component__["a" /* ServerListComponent */],
            __WEBPACK_IMPORTED_MODULE_14__arkmap_component__["a" /* ArkMapComponent */],
            __WEBPACK_IMPORTED_MODULE_21__sanitize_style_pipe__["a" /* SanitizeStylePipe */],
            __WEBPACK_IMPORTED_MODULE_22__sanitize_html_pipe__["a" /* SanitizeHtmlPipe */],
            __WEBPACK_IMPORTED_MODULE_23__clickOutside_directive__["a" /* ClickOutsideDirective */],
            __WEBPACK_IMPORTED_MODULE_9__player_player_component__["a" /* PlayerComponent */],
            __WEBPACK_IMPORTED_MODULE_10__player_menu_player_menu_component__["a" /* PlayerMenuComponent */],
            __WEBPACK_IMPORTED_MODULE_11__server_server_component__["a" /* ServerComponent */],
            __WEBPACK_IMPORTED_MODULE_13__admin_server_admin_server_component__["a" /* AdminServerComponent */],
            __WEBPACK_IMPORTED_MODULE_24__server_list_menu_server_list_menu_component__["a" /* ServerListMenuComponent */],
            __WEBPACK_IMPORTED_MODULE_25__menu_menu_component__["a" /* MenuComponent */],
            __WEBPACK_IMPORTED_MODULE_26__server_menu_server_menu_component__["a" /* ServerMenuComponent */],
            __WEBPACK_IMPORTED_MODULE_27__admin_server_menu_admin_server_menu_component__["a" /* AdminServerMenuComponent */],
            __WEBPACK_IMPORTED_MODULE_28__arkmap_structures_arkmap_structures_component__["a" /* ArkmapStructuresComponent */],
            __WEBPACK_IMPORTED_MODULE_29__timer_timer_component__["a" /* TimerComponent */],
            __WEBPACK_IMPORTED_MODULE_30__relative_time_relative_time_component__["a" /* RelativeTimeComponent */],
            __WEBPACK_IMPORTED_MODULE_31__confirm_button_confirm_button_component__["a" /* ConfirmButtonComponent */],
            __WEBPACK_IMPORTED_MODULE_32__access_denied_access_denied_component__["a" /* AccessDeniedComponent */],
            __WEBPACK_IMPORTED_MODULE_33__connection_error_connection_error_component__["a" /* ConnectionErrorComponent */],
            __WEBPACK_IMPORTED_MODULE_34__developer_developer_component__["a" /* DeveloperComponent */]
        ],
        imports: [
            __WEBPACK_IMPORTED_MODULE_4__angular_router__["a" /* RouterModule */].forRoot(appRoutes),
            __WEBPACK_IMPORTED_MODULE_7_ng2_breadcrumb_ng2_breadcrumb__["a" /* Ng2BreadcrumbModule */].forRoot(),
            __WEBPACK_IMPORTED_MODULE_0__angular_platform_browser__["a" /* BrowserModule */],
            __WEBPACK_IMPORTED_MODULE_3__angular_forms__["a" /* FormsModule */],
            __WEBPACK_IMPORTED_MODULE_5__angular_http__["a" /* HttpModule */],
            __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser_animations__["a" /* BrowserAnimationsModule */],
            __WEBPACK_IMPORTED_MODULE_6_angular2_notifications__["a" /* SimpleNotificationsModule */].forRoot(),
            __WEBPACK_IMPORTED_MODULE_35__data_table_data_table_module__["a" /* DataTableModule */]
        ],
        providers: [
            [{ provide: __WEBPACK_IMPORTED_MODULE_15__http_service__["a" /* HttpService */], useClass: !__WEBPACK_IMPORTED_MODULE_36__environments_environment__["a" /* environment */].demo ? __WEBPACK_IMPORTED_MODULE_15__http_service__["a" /* HttpService */] : __WEBPACK_IMPORTED_MODULE_16__demo_http_service__["a" /* DemoHttpService */] }],
            __WEBPACK_IMPORTED_MODULE_17__message_service__["a" /* MessageService */],
            __WEBPACK_IMPORTED_MODULE_18__data_service__["a" /* DataService */],
            __WEBPACK_IMPORTED_MODULE_19__data_resolver_service__["a" /* DataServiceResolver */],
            __WEBPACK_IMPORTED_MODULE_20__access_control_route_guard_service__["a" /* AccessControlRouteGuardService */],
            { provide: __WEBPACK_IMPORTED_MODULE_2__angular_core__["c" /* LOCALE_ID */], useValue: "en-US" }
        ],
        bootstrap: [__WEBPACK_IMPORTED_MODULE_8__app_component__["a" /* AppComponent */]]
    })
], AppModule);

//# sourceMappingURL=app.module.js.map

/***/ }),

/***/ 300:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__ = __webpack_require__(35);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__environments_environment__ = __webpack_require__(22);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5_d3__ = __webpack_require__(88);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5_d3___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_5_d3__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_moment__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_moment___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_6_moment__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ArkmapStructuresComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







var ArkmapStructuresComponent = (function () {
    function ArkmapStructuresComponent(dataService, httpService, zone) {
        this.dataService = dataService;
        this.httpService = httpService;
        this.zone = zone;
        this._structures = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
        this.keysGetter = Object.keys;
        this.ownerSortField = "locations";
        this.ownerSortFunctions = {
            "locations": function (o1, o2) {
                if (o1.AreaCount > o2.AreaCount) {
                    return -1;
                }
                else if (o1.AreaCount < o2.AreaCount) {
                    return 1;
                }
                else {
                    if (o1.StructureCount > o2.StructureCount) {
                        return -1;
                    }
                    else if (o1.StructureCount < o2.StructureCount) {
                        return 1;
                    }
                    else {
                        return 0;
                    }
                }
            },
            "structures": function (o1, o2) {
                if (o1.StructureCount > o2.StructureCount) {
                    return -1;
                }
                else if (o1.StructureCount < o2.StructureCount) {
                    return 1;
                }
                else {
                    return 0;
                }
            },
            "lastactive": function (o1, o2) {
                if (o1.LastActiveTime < o2.LastActiveTime || o1.LastActiveTime == undefined) {
                    return -1;
                }
                else if (o1.LastActiveTime > o2.LastActiveTime || o2.LastActiveTime == undefined) {
                    return 1;
                }
                else {
                    return 0;
                }
            }
        };
        this.width = 1024;
        this.height = 1024;
        this.zoom = __WEBPACK_IMPORTED_MODULE_5_d3__["zoom"]().scaleExtent([1, 8]);
    }
    Object.defineProperty(ArkmapStructuresComponent.prototype, "structures", {
        get: function () {
            return this._structures.getValue();
        },
        set: function (value) {
            this._structures.next(value);
        },
        enumerable: true,
        configurable: true
    });
    ;
    ArkmapStructuresComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._structuresSubscription = this._structures.subscribe(function (value) { return _this.update(value); });
        var element = this.mapContainer.nativeElement;
        this.map = {};
        this.map.canvas = __WEBPACK_IMPORTED_MODULE_5_d3__["select"](element)
            .append('canvas')
            .attr('width', 1024)
            .attr('height', 1024)
            .node().getContext('2d');
        this.map.svg = __WEBPACK_IMPORTED_MODULE_5_d3__["select"](element)
            .append('svg')
            .attr('viewBox', '0 0 1024 1024')
            .attr('preserveAspectRatio', 'xMidYMid')
            .append('g')
            .on("contextmenu", function (d, e) {
            __WEBPACK_IMPORTED_MODULE_5_d3__["event"].preventDefault();
        });
        /*this.map.tooltip = d3.select(document)
          .append("div")
          .style("position", "absolute")
          .style("z-index", "20")
          .style("visibility", "hidden")
          .text("");*/
        this.map.svg.append('rect')
            .attr('class', 'overlay')
            .attr('width', 1024)
            .attr('height', 1024);
        this.map.x =
            __WEBPACK_IMPORTED_MODULE_5_d3__["scaleLinear"]()
                .domain([0, 1024])
                .range([0, 1024]);
        this.map.y =
            __WEBPACK_IMPORTED_MODULE_5_d3__["scaleLinear"]()
                .domain([0, 1024])
                .range([0, 1024]);
        __WEBPACK_IMPORTED_MODULE_5_d3__["select"](element).call(this.zoom.on('zoom', function () {
            _this.hideContextMenu();
            _this.redraw();
        })).on('wheel.zoom', null);
        if (this.structures)
            this.updateMap();
    };
    ArkmapStructuresComponent.prototype.ngOnDestroy = function () {
        this._structuresSubscription.unsubscribe();
    };
    ArkmapStructuresComponent.prototype.zoomIn = function () {
        this.zoom.scaleBy(__WEBPACK_IMPORTED_MODULE_5_d3__["select"](this.mapContainer.nativeElement), 1.2);
    };
    ArkmapStructuresComponent.prototype.zoomOut = function () {
        this.zoom.scaleBy(__WEBPACK_IMPORTED_MODULE_5_d3__["select"](this.mapContainer.nativeElement), 0.8);
    };
    ArkmapStructuresComponent.prototype.updateSelection = function () {
        var _this = this;
        this.map.svg.circle.attr("display", function (d) {
            var owner = _this.structures.Owners[d.OwnerId];
            return !d.Removed && !owner.Removed && (!_this.selectedOwner || (_this.selectedOwner && _this.selectedOwner.Id == d.OwnerId)) ? 'block' : 'none';
        });
        this.redraw();
    };
    ArkmapStructuresComponent.prototype.update = function (structures) {
        this.sortOwners(structures);
        if (this.map)
            this.updateMap();
    };
    ArkmapStructuresComponent.prototype.sortOwners = function (structures) {
        var sortFunc = this.ownerSortFunctions[this.ownerSortField];
        if (structures) {
            var owners = structures.Owners.slice();
            owners.sort(sortFunc);
            this.ownersSorted = owners;
        }
        else
            this.ownersSorted = undefined;
    };
    ArkmapStructuresComponent.prototype.updateMap = function () {
        var _this = this;
        this.map.svg.nodes = this.structures.Areas;
        this.map.svg.draw = function () {
            _this.map.svg.circle = _this.map.svg.selectAll('circle')
                .data(_this.map.svg.nodes).enter()
                .append('circle')
                .attr('r', function (d) { return d.RadiusPx < 2 ? 2 : d.RadiusPx; })
                .attr('fill', 'transparent')
                .attr('stroke', function (d) {
                var owner = _this.structures.Owners[d.OwnerId];
                var active = owner.LastActiveTime ? __WEBPACK_IMPORTED_MODULE_6_moment__(new Date(owner.LastActiveTime)).isSameOrAfter(__WEBPACK_IMPORTED_MODULE_6_moment__().subtract(28, 'day')) : false;
                return active && (d.StructureCount >= 100 || (d.TrashQuota < 0.5 && d.StructureCount >= 10)) ? 'magenta' : 'red';
            })
                .attr('stroke-width', function (d) {
                var owner = _this.structures.Owners[d.OwnerId];
                var active = owner.LastActiveTime ? __WEBPACK_IMPORTED_MODULE_6_moment__(new Date(owner.LastActiveTime)).isSameOrAfter(__WEBPACK_IMPORTED_MODULE_6_moment__().subtract(28, 'day')) : false;
                return active && (d.StructureCount >= 100 || (d.TrashQuota < 0.5 && d.StructureCount >= 10)) ? 3 : 2;
            })
                .attr('transform', _this.map.svg.transform);
            /*circle.on("mouseover", (d) => {
              var owner = this.structures.Owners[d.OwnerId];
              this.map.tooltip.text(owner.Name + ": " + d.StructureCount + " structures\n"
                + d.Latitude + ", " + d.Longitude);
              return this.map.tooltip.style("visibility", "visible");
            })
              .on("mousemove", (d) => {
                let p = d3.mouse(d3.event.currentTarget);
                console.log(p);
                return this.map.tooltip.style("top", x.invert(p[1])+"px").style("left",y.invert(p[0])+"px");})
              .on("mouseout", (d) => {return this.map.tooltip.style("visibility", "hidden");});*/
            _this.map.svg.circle.on("click", function (d) {
                __WEBPACK_IMPORTED_MODULE_5_d3__["event"].preventDefault();
                var p = {};
                p.x = __WEBPACK_IMPORTED_MODULE_5_d3__["event"].pageX;
                p.y = __WEBPACK_IMPORTED_MODULE_5_d3__["event"].pageY;
                _this.showAreaModal(d, p);
            });
            _this.map.svg.circle.append("svg:title")
                .text(function (d) {
                var owner = _this.structures.Owners[d.OwnerId];
                var lastActiveTime = owner.LastActiveTime ? __WEBPACK_IMPORTED_MODULE_6_moment__(new Date(owner.LastActiveTime)).fromNow() : null;
                return owner.Name + ": " + d.StructureCount + " structures\n"
                    + "Coords: " + d.Latitude + ", " + d.Longitude + "\n"
                    + (lastActiveTime ? "Last active: " + lastActiveTime + "\n" : "")
                    + "---\n"
                    + d.Structures.map(function (s) {
                        var type = _this.structures.Types[s.t];
                        return s.c + ": " + (type ? type.Name : s.t);
                    }).join("\n");
            });
        };
        this.map.svg.draw();
        this.map.svg.transform = function (d) {
            return 'translate(' + _this.map.x(d.TopoMapX) + ',' + _this.map.y(d.TopoMapY) + ')';
        };
        this.map.svg.circle.attr('transform', this.map.svg.transform);
    };
    ArkmapStructuresComponent.prototype.imageLoaded = function (img) {
        var _this = this;
        this.img = img;
        this.width = img ? img.naturalWidth : 1024;
        this.height = img ? img.naturalHeight : 1024;
        //d3.select(this.canvasRef.nativeElement).call(this.zoom.on("zoom", () => this.zoomed()));
        window.setTimeout(function () { _this.resize(); _this.redraw(); }, 100);
    };
    ArkmapStructuresComponent.prototype.resize = function () {
        //this.zoom.translateExtent([[0, 0], [this.width, this.height]]);
    };
    ArkmapStructuresComponent.prototype.redraw = function () {
        var _this = this;
        var transform = __WEBPACK_IMPORTED_MODULE_5_d3__["zoomTransform"](this.mapContainer.nativeElement);
        this.map.svg.attr("transform", "translate(" + transform.x + "," + transform.y + ") scale(" + transform.k + ")");
        if (transform.k != this.prevTransformK) {
            this.map.svg.circle.attr("stroke-width", function (d) {
                var owner = _this.structures.Owners[d.OwnerId];
                var active = owner.LastActiveTime ? __WEBPACK_IMPORTED_MODULE_6_moment__(new Date(owner.LastActiveTime)).isSameOrAfter(__WEBPACK_IMPORTED_MODULE_6_moment__().subtract(28, 'day')) : false;
                return (active && (d.StructureCount >= 100 || (d.TrashQuota < 0.5 && d.StructureCount >= 10)) ? 3 : 2) / transform.k;
            });
        }
        var ctx = this.map.canvas;
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.clearRect(0, 0, 1024, 1024);
        ctx.translate(transform.x, transform.y);
        ctx.scale(transform.k, transform.k);
        if (this.img)
            ctx.drawImage(this.img, 0, 0);
        this.prevTransformK = transform.k;
    };
    ArkmapStructuresComponent.prototype.ngOnChanges = function (changes) {
        var _this = this;
        if (this.mapName == null)
            return;
        var img = new Image();
        img.onload = function () { return _this.imageLoaded(img); };
        img.onerror = function () { return _this.imageLoaded(undefined); };
        img.src = !__WEBPACK_IMPORTED_MODULE_4__environments_environment__["a" /* environment */].demo ? this.getApiBaseUrl() + "/map/" + this.mapName : 'assets/demo/Ragnarok.jpg';
        if (img.complete) {
            img.onload = null;
            img.onerror = null;
            this.imageLoaded(img);
        }
    };
    ArkmapStructuresComponent.prototype.getApiBaseUrl = function () {
        return __WEBPACK_IMPORTED_MODULE_4__environments_environment__["a" /* environment */].apiBaseUrl
            .replace(/\<protocol\>/gi, window.location.protocol)
            .replace(/\<hostname\>/gi, window.location.hostname)
            .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    };
    ArkmapStructuresComponent.prototype.reset = function () {
        this.selectedOwner = undefined;
        this.updateSelection();
        //this.zoom.scaleTo(d3.select(this.mapContainer.nativeElement), 1.0);
        //this.zoom.translateTo(d3.select(this.mapContainer.nativeElement), 0, 0); //not working
    };
    ArkmapStructuresComponent.prototype.setSelectedOwner = function (owner) {
        this.selectedOwner = owner;
        this.updateSelection();
    };
    ArkmapStructuresComponent.prototype.setOwnerSort = function (field) {
        this.ownerSortField = field;
        this.sortOwners(this.structures);
    };
    ArkmapStructuresComponent.prototype.showAreaModal = function (area, point) {
        this.currentArea = area;
        this.currentOwner = this.structures.Owners[area.OwnerId];
        var cm = __WEBPACK_IMPORTED_MODULE_5_d3__["select"](this.contextMenu.nativeElement);
        cm.style("display", "block");
        if (__WEBPACK_IMPORTED_MODULE_5_d3__["event"])
            __WEBPACK_IMPORTED_MODULE_5_d3__["event"].stopPropagation();
    };
    ArkmapStructuresComponent.prototype.showOwnerModal = function (event, owner) {
        this.currentOwner = owner;
        var cm = __WEBPACK_IMPORTED_MODULE_5_d3__["select"](this.contextMenu.nativeElement);
        cm.style("display", "block");
        event.stopPropagation();
    };
    ArkmapStructuresComponent.prototype.showInfoModal = function (header, message) {
        var modalInfo = {};
        modalInfo.Header = header;
        modalInfo.Message = message;
        this.modalInfo = modalInfo;
        var cm = __WEBPACK_IMPORTED_MODULE_5_d3__["select"](this.contextMenu.nativeElement);
        cm.style("display", "block");
        if (__WEBPACK_IMPORTED_MODULE_5_d3__["event"])
            __WEBPACK_IMPORTED_MODULE_5_d3__["event"].stopPropagation();
    };
    ArkmapStructuresComponent.prototype.hideContextMenu = function () {
        var cm = __WEBPACK_IMPORTED_MODULE_5_d3__["select"](this.contextMenu.nativeElement);
        cm.style("display", "none");
        this.currentArea = undefined;
        this.currentOwner = undefined;
        this.modalInfo = undefined;
    };
    ArkmapStructuresComponent.prototype.destroyCurrentArea = function (event) {
        var _this = this;
        this.httpService.adminDestroyStructuresForTeamIdAtPosition(this.serverKey, this.currentOwner.OwnerId, this.currentArea.X, this.currentArea.Y, +this.currentArea.RadiusUu + 1000 /* 10m */, 1)
            .then(function (response) {
            _this.currentArea.Removed = true;
            _this.currentOwner.AreaCount -= 1;
            //if(response.DestroyedStructureCount) this.currentOwner.StructureCount -= response.DestroyedStructureCount; //this does not work well because of server not saving which areas have been demolished inbetween updates
            _this.currentOwner.StructureCount -= _this.currentArea.StructureCount;
            _this.hideContextMenu();
            _this.showInfoModal("Action Successfull!", response.Message);
            _this.updateSelection();
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    ArkmapStructuresComponent.prototype.destroyAllStructuresForTeam = function (event) {
        var _this = this;
        this.httpService.adminDestroyAllStructuresForTeamId(this.serverKey, this.currentOwner.OwnerId)
            .then(function (response) {
            _this.currentOwner.Removed = true;
            _this.currentOwner.AreaCount = 0;
            _this.currentOwner.StructureCount = 0;
            //if(response.DestroyedStructureCount) owner.StructureCount -= response.DestroyedStructureCount; //this does not work well because of server not saving which areas have been demolished inbetween updates
            _this.hideContextMenu();
            _this.showInfoModal("Action Successfull!", response.Message);
            _this.updateSelection();
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    ArkmapStructuresComponent.prototype.destroyDinosForTeam = function (event) {
        var _this = this;
        this.httpService.adminDestroyDinosForTeamId(this.serverKey, this.currentOwner.OwnerId)
            .then(function (response) {
            _this.currentOwner.CreatureCount = 0;
            _this.hideContextMenu();
            _this.showInfoModal("Action Successfull!", response.Message);
            _this.updateSelection();
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    ArkmapStructuresComponent.prototype.saveWorld = function (event) {
        var _this = this;
        this.httpService.adminSaveWorld(this.serverKey)
            .then(function (response) {
            _this.hideContextMenu();
            _this.showInfoModal("Action Successfull!", response.Message);
        })
            .catch(function (error) {
            _this.hideContextMenu();
            var json = error && error._body ? JSON.parse(error._body) : undefined;
            _this.showInfoModal("Action Failed...", json ? json.Message : error.statusText);
        });
    };
    return ArkmapStructuresComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object),
    __metadata("design:paramtypes", [Object])
], ArkmapStructuresComponent.prototype, "structures", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], ArkmapStructuresComponent.prototype, "serverKey", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], ArkmapStructuresComponent.prototype, "mapName", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('map'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */]) === "function" && _a || Object)
], ArkmapStructuresComponent.prototype, "mapContainer", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('contextMenu'),
    __metadata("design:type", typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */]) === "function" && _b || Object)
], ArkmapStructuresComponent.prototype, "contextMenu", void 0);
ArkmapStructuresComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'arkmap-structures',
        template: __webpack_require__(402),
        styles: [__webpack_require__(380)],
        encapsulation: __WEBPACK_IMPORTED_MODULE_0__angular_core__["X" /* ViewEncapsulation */].None
    }),
    __metadata("design:paramtypes", [typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */]) === "function" && _c || Object, typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_3__http_service__["a" /* HttpService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__http_service__["a" /* HttpService */]) === "function" && _d || Object, typeof (_e = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["V" /* NgZone */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["V" /* NgZone */]) === "function" && _e || Object])
], ArkmapStructuresComponent);

var _a, _b, _c, _d, _e;
//# sourceMappingURL=arkmap-structures.component.js.map

/***/ }),

/***/ 301:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__environments_environment__ = __webpack_require__(22);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_d3__ = __webpack_require__(88);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_d3___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_d3__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ArkMapComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};



var ArkMapComponent = (function () {
    function ArkMapComponent() {
        this.width = 1024;
        this.height = 1024;
        this.zoom = __WEBPACK_IMPORTED_MODULE_2_d3__["zoom"]().scaleExtent([1, 10]);
    }
    ArkMapComponent.prototype.imageLoaded = function (img) {
        var _this = this;
        this.img = img;
        this.width = img.naturalWidth;
        this.height = img.naturalHeight;
        //d3.select(this.canvasRef.nativeElement).call(this.zoom.on("zoom", () => this.zoomed()));
        window.setTimeout(function () { _this.resize(); _this.redraw(); }, 100);
    };
    ArkMapComponent.prototype.resize = function () {
        //this.zoom.translateExtent([[0, 0], [this.width, this.height]]);
    };
    ArkMapComponent.prototype.zoomed = function () {
        var transform = __WEBPACK_IMPORTED_MODULE_2_d3__["zoomTransform"](this.canvasRef.nativeElement);
        var ctx = this.canvasRef.nativeElement.getContext('2d');
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.clearRect(0, 0, this.width, this.height);
        ctx.translate(transform.x, transform.y);
        ctx.scale(transform.k, transform.k);
        this.redraw();
    };
    ArkMapComponent.prototype.redraw = function () {
        var ctx = this.canvasRef.nativeElement.getContext('2d');
        ctx.drawImage(this.img, 0, 0);
        if (this.points == null)
            return;
        for (var _i = 0, _a = this.points; _i < _a.length; _i++) {
            var point = _a[_i];
            ctx.beginPath();
            ctx.arc(point.x, point.y, 7, 0, Math.PI * 2);
            ctx.fillStyle = 'black';
            ctx.fill();
            ctx.lineWidth = 2;
            ctx.strokeStyle = 'white';
            ctx.stroke();
        }
    };
    ArkMapComponent.prototype.ngOnChanges = function (changes) {
        var _this = this;
        if (this.mapName == null)
            return;
        var img = new Image();
        img.onload = function () { return _this.imageLoaded(img); };
        img.src = !__WEBPACK_IMPORTED_MODULE_1__environments_environment__["a" /* environment */].demo ? this.getApiBaseUrl() + "/map/" + this.mapName : 'assets/demo/Ragnarok.jpg';
        if (img.complete) {
            img.onload = null;
            this.imageLoaded(img);
        }
    };
    ArkMapComponent.prototype.getApiBaseUrl = function () {
        return __WEBPACK_IMPORTED_MODULE_1__environments_environment__["a" /* environment */].apiBaseUrl
            .replace(/\<protocol\>/gi, window.location.protocol)
            .replace(/\<hostname\>/gi, window.location.hostname)
            .replace(/\<webapi_port\>/gi, typeof config !== 'undefined' ? config.webapi.port : "");
    };
    return ArkMapComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], ArkMapComponent.prototype, "mapName", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Array)
], ArkMapComponent.prototype, "points", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('myCanvas'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */]) === "function" && _a || Object)
], ArkMapComponent.prototype, "canvasRef", void 0);
ArkMapComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'arkmap',
        template: "<canvas #myCanvas [width]=\"width\" [height]=\"height\" style=\"width: 100%;\"></canvas>"
    }),
    __metadata("design:paramtypes", [])
], ArkMapComponent);

var _a;
//# sourceMappingURL=arkmap.component.js.map

/***/ }),

/***/ 302:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ClickOutsideDirective; });
//from: https://github.com/chliebel/angular2-click-outside
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};

var ClickOutsideDirective = (function () {
    function ClickOutsideDirective(_elementRef) {
        this._elementRef = _elementRef;
        this.clickOutside = new __WEBPACK_IMPORTED_MODULE_0__angular_core__["t" /* EventEmitter */]();
    }
    ClickOutsideDirective.prototype.onClick = function (event, targetElement) {
        if (!targetElement) {
            return;
        }
        var clickedInside = this._elementRef.nativeElement.contains(targetElement);
        if (!clickedInside) {
            this.clickOutside.emit(event);
        }
    };
    return ClickOutsideDirective;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["x" /* Output */])(),
    __metadata("design:type", Object)
], ClickOutsideDirective.prototype, "clickOutside", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["p" /* HostListener */])('document:click', ['$event', '$event.target']),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, Object]),
    __metadata("design:returntype", void 0)
], ClickOutsideDirective.prototype, "onClick", null);
ClickOutsideDirective = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["k" /* Directive */])({
        selector: '[clickOutside]'
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */]) === "function" && _a || Object])
], ClickOutsideDirective);

var _a;
//# sourceMappingURL=clickOutside.directive.js.map

/***/ }),

/***/ 303:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ConfirmButtonComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};

var ConfirmButtonComponent = (function () {
    function ConfirmButtonComponent() {
        this.callback = new __WEBPACK_IMPORTED_MODULE_0__angular_core__["t" /* EventEmitter */]();
        this.confirming = false;
    }
    ConfirmButtonComponent.prototype.ngOnInit = function () {
    };
    ConfirmButtonComponent.prototype.onClick = function (event) {
        var _this = this;
        if (!this.confirming) {
            this.confirming = true;
            this.resetTimeout = window.setTimeout(function () {
                _this.confirming = false;
            }, 5000);
        }
        else {
            if (event.detail >= 3) {
                window.clearTimeout(this.resetTimeout);
                this.confirming = false;
                this.callback.emit();
            }
        }
    };
    return ConfirmButtonComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["x" /* Output */])(),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["t" /* EventEmitter */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["t" /* EventEmitter */]) === "function" && _a || Object)
], ConfirmButtonComponent.prototype, "callback", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Number)
], ConfirmButtonComponent.prototype, "width", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('confirmButton'),
    __metadata("design:type", typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["n" /* ElementRef */]) === "function" && _b || Object)
], ConfirmButtonComponent.prototype, "confirmButton", void 0);
ConfirmButtonComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'confirm-button',
        template: __webpack_require__(403),
        styles: [__webpack_require__(381)]
    }),
    __metadata("design:paramtypes", [])
], ConfirmButtonComponent);

var _a, _b;
//# sourceMappingURL=confirm-button.component.js.map

/***/ }),

/***/ 304:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__message_service__ = __webpack_require__(19);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ConnectionErrorComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var ConnectionErrorComponent = (function () {
    function ConnectionErrorComponent(dataService, messageService, notificationsService) {
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.menuOption = undefined;
    }
    ConnectionErrorComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) { return _this.showServerUpdateNotification(serverKey); });
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) { return _this.menuOption = menuOption; });
    };
    ConnectionErrorComponent.prototype.ngOnDestroy = function () {
        this.serverUpdatedSubscription.unsubscribe();
        this.menuOptionSubscription.unsubscribe();
    };
    ConnectionErrorComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    ConnectionErrorComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    return ConnectionErrorComponent;
}());
ConnectionErrorComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-connection-error',
        template: __webpack_require__(404),
        styles: [__webpack_require__(382)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _c || Object])
], ConnectionErrorComponent);

var _a, _b, _c;
//# sourceMappingURL=connection-error.component.js.map

/***/ }),

/***/ 305:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DataServiceResolver; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};



var DataServiceResolver = (function () {
    function DataServiceResolver(dataService, router) {
        this.dataService = dataService;
        this.router = router;
    }
    DataServiceResolver.prototype.resolve = function (route, state) {
        var _this = this;
        return this.dataService.getServers()
            .then(function (servers) {
            return _this.dataService;
        })
            .catch(function (error) {
            return _this.dataService;
        });
    };
    return DataServiceResolver;
}());
DataServiceResolver = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["d" /* Injectable */])(),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */]) === "function" && _b || Object])
], DataServiceResolver);

var _a, _b;
//# sourceMappingURL=data-resolver.service.js.map

/***/ }),

/***/ 306:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__columns_column_directive__ = __webpack_require__(105);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__modes_mode_directive__ = __webpack_require__(106);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__ = __webpack_require__(35);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4_rxjs_add_observable_combineLatest__ = __webpack_require__(241);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4_rxjs_add_observable_combineLatest___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_4_rxjs_add_observable_combineLatest__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5_rxjs_add_observable_of__ = __webpack_require__(242);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5_rxjs_add_observable_of___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_5_rxjs_add_observable_of__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_rxjs_add_operator_catch__ = __webpack_require__(243);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6_rxjs_add_operator_catch___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_6_rxjs_add_operator_catch__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7_rxjs_add_operator_switchMap__ = __webpack_require__(246);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7_rxjs_add_operator_switchMap___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_7_rxjs_add_operator_switchMap__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8_rxjs_add_operator_filter__ = __webpack_require__(244);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_8_rxjs_add_operator_filter___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_8_rxjs_add_operator_filter__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_9_rxjs_add_operator_startWith__ = __webpack_require__(245);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_9_rxjs_add_operator_startWith___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_9_rxjs_add_operator_startWith__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DataTableComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};










var DataTableComponent = (function () {
    function DataTableComponent(ref) {
        this.ref = ref;
        this._modeEnabledSubscriptions = [];
        this._rows$ = __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["Observable"].of([]);
        this._orderByColumnKey = new __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["BehaviorSubject"](undefined);
        this._filter = new __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["BehaviorSubject"](undefined);
        this._sort = new __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["Subject"]();
        this._fromRow = 0;
        this._numRows = 25;
        this._totalRows = 0;
        this._enabledColumnsForMode = {};
        this._viewOptions = [
            { 'value': 25, 'text': '25' },
            { 'value': 50, 'text': '50' },
            { 'value': 100, 'text': '100' },
            { 'value': 250, 'text': '250' },
            { 'value': 500, 'text': '500' },
            { 'value': 1000, 'text': '1000' },
            { 'value': 1000000, 'text': 'All' }
        ];
        this._prevColumnKey = undefined;
        this._prevFilter = undefined;
        this._prevSortedRows = undefined;
        this._prevFilteredRows = undefined;
        this._prevSortedRowsKey = undefined;
        this._prevFilteredRowsKey = undefined;
    }
    DataTableComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._rows$ = __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["Observable"].combineLatest(this._orderByColumnKey, this._filter.debounceTime(250), function (key, filter) { return ({ key: key, filter: filter }); })
            .skip(1)
            .startWith({ key: this._orderByColumnKey.getValue(), filter: this._filter.getValue() })
            .switchMap(function (x) {
            return __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["Observable"].of(_this.filterAndSortData(x.key, x.filter));
        })
            .catch(function (error) {
            console.log("Error in component ... " + error);
            return __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["Observable"].of(_this._rows);
        });
    };
    Object.defineProperty(DataTableComponent.prototype, "modeTemplates", {
        set: function (val) {
            if (!val)
                return;
            var arr = val.toArray();
            if (!arr.length)
                return;
            var result = [];
            for (var _i = 0, arr_1 = arr; _i < arr_1.length; _i++) {
                var temp = arr_1[_i];
                var mode = {};
                var props = Object.getOwnPropertyNames(temp);
                for (var _a = 0, props_1 = props; _a < props_1.length; _a++) {
                    var prop = props_1[_a];
                    mode[prop] = temp[prop];
                }
                result.push(mode);
            }
            this._modes = result;
            this.numEnabledModes = __WEBPACK_IMPORTED_MODULE_3_rxjs_Rx__["Observable"].combineLatest(result.map(function (r) { return r.enabled; }))
                .map(function (r) { return r.map(function (v) { return v ? 1 : 0; }).reduce(function (s, v) { return s + v; }); });
            if (this._modes.length > 0)
                this._currentMode = this._modes[0].key;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DataTableComponent.prototype, "columnTemplates", {
        set: function (val) {
            if (!val)
                return;
            var arr = val.toArray();
            if (!arr.length)
                return;
            var result = [];
            for (var _i = 0, arr_2 = arr; _i < arr_2.length; _i++) {
                var temp = arr_2[_i];
                var col = {};
                var props = Object.getOwnPropertyNames(temp);
                for (var _a = 0, props_2 = props; _a < props_2.length; _a++) {
                    var prop = props_2[_a];
                    col[prop] = temp[prop];
                }
                if (temp.headerTemplate) {
                    col.headerTemplate = temp.headerTemplate;
                }
                if (temp.cellTemplate) {
                    col.cellTemplate = temp.cellTemplate;
                }
                result.push(col);
            }
            this._columnTemplates = result;
            this.orderBy(this.orderByColumn);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DataTableComponent.prototype, "rows", {
        set: function (val) {
            this._rows = val;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DataTableComponent.prototype, "trackByProp", {
        set: function (val) {
            var _this = this;
            this._trackByProp = val;
            this.trackByRow = function (index, row) {
                return row[_this._trackByProp];
            };
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DataTableComponent.prototype, "filter", {
        set: function (val) {
            this._filter.next(val);
        },
        enumerable: true,
        configurable: true
    });
    ;
    DataTableComponent.prototype.isCurrentMode = function (key) {
        return key === this._currentMode;
    };
    DataTableComponent.prototype.setCurrentMode = function (key) {
        this._currentMode = key;
    };
    DataTableComponent.prototype.showColumn = function (columnKey) {
        var _this = this;
        if ((this._enabledColumnsForMode[this._currentMode] || (this._enabledColumnsForMode[this._currentMode] = {}))[columnKey] === true)
            return true;
        if (!this._currentMode)
            return false;
        var currentMode = this._modes.find(function (x) { return x.key == _this._currentMode; });
        if (!currentMode)
            return false;
        var enabled = currentMode.ColumnKeys.find(function (x) { return x == columnKey; }) != undefined;
        this._enabledColumnsForMode[this._currentMode][columnKey] = enabled;
        return enabled;
    };
    DataTableComponent.prototype.currentModeEnabledColumnCount = function () {
        var count = 0;
        for (var i = 0; i < this._columnTemplates.length; i++) {
            if (this.showColumn(this._columnTemplates[i].key))
                count++;
        }
        return count;
    };
    DataTableComponent.prototype.trackByKey = function (index, data) {
        return data.key;
    };
    DataTableComponent.prototype.orderBy = function (columnKey, event) {
        if (event === void 0) { event = undefined; }
        var column = this._columnTemplates.find(function (x) { return x.key == columnKey && (event == undefined || x.orderBy == true); });
        if (!column)
            return;
        this._orderByColumnKey.next((this._orderByColumnKey.getValue() == columnKey ? '-' : '') + columnKey);
    };
    DataTableComponent.prototype.filterAndSortData = function (columnKey, filter) {
        var rows = undefined;
        if (filter != this._prevFilter && columnKey == this._prevColumnKey) {
            if (this._prevSortedRowsKey != columnKey) {
                this._prevSortedRowsKey = columnKey;
                this._prevSortedRows = this.sortData(this._rows.slice(), columnKey);
            }
            rows = this.filterData(this._prevSortedRows, filter);
        }
        else {
            if (this._prevFilteredRowsKey != filter) {
                this._prevFilteredRowsKey = filter;
                this._prevFilteredRows = this.filterData(this._rows, filter);
            }
            if (filter == undefined || filter == null || filter == "") {
                this._prevSortedRowsKey = columnKey;
                rows = this._prevSortedRows = this.sortData(this._rows.slice(), columnKey);
            }
            else
                rows = this.sortData(this._prevFilteredRows, columnKey);
        }
        if (filter != this._prevFilter)
            this.setFirstPage();
        this._totalRows = rows.length;
        this._prevColumnKey = columnKey;
        this._prevFilter = filter;
        return rows;
    };
    DataTableComponent.prototype.filterData = function (rows, filter) {
        var _this = this;
        if (filter == undefined || filter == null || filter == "")
            return rows.slice();
        return rows.filter(function (x) { return _this.filterFunction(x, filter); });
    };
    DataTableComponent.prototype.sortData = function (rows, columnKey) {
        var _this = this;
        if (columnKey == undefined)
            return rows;
        var asc = columnKey[0] != '-';
        var column = this._columnTemplates.find(function (x) { return x.key == columnKey.substr(asc ? 0 : 1); });
        var sortFunc = this.sortFunctions[column.key.replace(/^\-/, "")];
        var alts = (column.thenBy || '').split(',').filter(function (k) { return _this.sortFunctions.hasOwnProperty(k.replace(/^\-/, "")); }).map(function (k) {
            var a = {};
            a.asc = k[0] != '-';
            a.sortFunc = _this.sortFunctions[k.replace(/^\-/, "")];
            return a;
        });
        return rows.sort(function (o1, o2) {
            var r = sortFunc(o1, o2, asc);
            if (r == 0) {
                for (var _i = 0, alts_1 = alts; _i < alts_1.length; _i++) {
                    var alt = alts_1[_i];
                    r = alt.sortFunc(o1, o2, alt.asc);
                    if (r != 0)
                        break;
                }
            }
            return r;
        });
    };
    DataTableComponent.prototype.setViewOffset = function (offset) {
        var newOffset = offset;
        if (newOffset < 0)
            newOffset = 0;
        if (newOffset >= this._totalRows)
            newOffset = this._totalRows - 1;
        this._fromRow = parseInt("" + newOffset);
        this.ref.markForCheck();
    };
    DataTableComponent.prototype.setViewOffsetRelative = function (offset) {
        this.setViewOffset(this._fromRow + offset);
    };
    DataTableComponent.prototype.setFirstPage = function () {
        if (!this.isFirstPage())
            this.setViewOffset(0);
    };
    DataTableComponent.prototype.setPrevPage = function () {
        if (!this.isFirstPage())
            this.setViewOffsetRelative(-this._numRows);
    };
    DataTableComponent.prototype.setNextPage = function () {
        if (!this.isLastPage())
            this.setViewOffsetRelative(this._numRows);
    };
    DataTableComponent.prototype.setLastPage = function () {
        if (!this.isLastPage())
            this.setViewOffset(this._totalRows - this._numRows);
    };
    DataTableComponent.prototype.isFirstPage = function () {
        return this._fromRow <= 0;
    };
    DataTableComponent.prototype.isLastPage = function () {
        return this._fromRow >= this._totalRows - this._numRows;
    };
    DataTableComponent.prototype.setViewLimit = function (limit) {
        this._numRows = parseInt("" + (limit > 0 ? limit : 1000000));
        this.ref.markForCheck();
    };
    DataTableComponent.prototype.getLastRowOffset = function () {
        var last = this._fromRow + this._numRows;
        return last > this._totalRows ? this._totalRows : last;
    };
    return DataTableComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["s" /* ContentChildren */])(__WEBPACK_IMPORTED_MODULE_2__modes_mode_directive__["a" /* DataTableModeDirective */]),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */]) === "function" && _a || Object),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */]) === "function" && _b || Object])
], DataTableComponent.prototype, "modeTemplates", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["s" /* ContentChildren */])(__WEBPACK_IMPORTED_MODULE_1__columns_column_directive__["a" /* DataTableColumnDirective */]),
    __metadata("design:type", typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */]) === "function" && _c || Object),
    __metadata("design:paramtypes", [typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["_11" /* QueryList */]) === "function" && _d || Object])
], DataTableComponent.prototype, "columnTemplates", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object),
    __metadata("design:paramtypes", [Object])
], DataTableComponent.prototype, "rows", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String),
    __metadata("design:paramtypes", [String])
], DataTableComponent.prototype, "trackByProp", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String),
    __metadata("design:paramtypes", [String])
], DataTableComponent.prototype, "filter", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object)
], DataTableComponent.prototype, "filterFunction", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object)
], DataTableComponent.prototype, "sortFunctions", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", String)
], DataTableComponent.prototype, "orderByColumn", void 0);
DataTableComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'ark-data-table',
        template: __webpack_require__(405),
        styles: [__webpack_require__(383)],
        changeDetection: __WEBPACK_IMPORTED_MODULE_0__angular_core__["_13" /* ChangeDetectionStrategy */].OnPush,
        encapsulation: __WEBPACK_IMPORTED_MODULE_0__angular_core__["X" /* ViewEncapsulation */].None
    }),
    __metadata("design:paramtypes", [typeof (_e = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */]) === "function" && _e || Object])
], DataTableComponent);

var _a, _b, _c, _d, _e;
//# sourceMappingURL=data-table.component.js.map

/***/ }),

/***/ 307:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_common__ = __webpack_require__(37);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__angular_forms__ = __webpack_require__(103);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__data_table_component__ = __webpack_require__(306);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__ = __webpack_require__(105);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__modes_mode_directive__ = __webpack_require__(106);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DataTableModule; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};






var DataTableModule = (function () {
    function DataTableModule() {
    }
    return DataTableModule;
}());
DataTableModule = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["b" /* NgModule */])({
        imports: [
            __WEBPACK_IMPORTED_MODULE_1__angular_common__["i" /* CommonModule */],
            __WEBPACK_IMPORTED_MODULE_2__angular_forms__["a" /* FormsModule */]
        ],
        providers: [],
        declarations: [
            __WEBPACK_IMPORTED_MODULE_3__data_table_component__["a" /* DataTableComponent */],
            __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__["a" /* DataTableColumnDirective */],
            __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__["b" /* DataTableColumnHeaderDirective */],
            __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__["c" /* DataTableColumnCellDirective */],
            __WEBPACK_IMPORTED_MODULE_5__modes_mode_directive__["a" /* DataTableModeDirective */]
        ],
        exports: [
            __WEBPACK_IMPORTED_MODULE_3__data_table_component__["a" /* DataTableComponent */],
            __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__["a" /* DataTableColumnDirective */],
            __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__["b" /* DataTableColumnHeaderDirective */],
            __WEBPACK_IMPORTED_MODULE_4__columns_column_directive__["c" /* DataTableColumnCellDirective */],
            __WEBPACK_IMPORTED_MODULE_5__modes_mode_directive__["a" /* DataTableModeDirective */]
        ]
    })
], DataTableModule);

//# sourceMappingURL=data-table.module.js.map

/***/ }),

/***/ 308:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_http__ = __webpack_require__(68);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_rxjs_add_operator_toPromise__ = __webpack_require__(93);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_rxjs_add_operator_toPromise___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_rxjs_add_operator_toPromise__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__http_service__ = __webpack_require__(29);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DemoHttpService; });
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var DemoHttpService = (function (_super) {
    __extends(DemoHttpService, _super);
    function DemoHttpService(http) {
        return _super.call(this, http) || this;
    }
    DemoHttpService.prototype.getServers = function () {
        return this.http.get('assets/demo/servers.json')
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    DemoHttpService.prototype.getServer = function (serverKey) {
        return this.http.get('assets/demo/server.json')
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    DemoHttpService.prototype.getWildCreatures = function (serverKey) {
        return this.http.get('assets/demo/wildcreatures.json')
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    DemoHttpService.prototype.getStructures = function (serverKey) {
        return this.http.get('assets/demo/structures.json')
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    DemoHttpService.prototype.getPlayer = function (steamId) {
        return this.http.get('assets/demo/player.json')
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    DemoHttpService.prototype.getAdminServer = function (serverKey) {
        return this.http.get('assets/demo/adminserver.json')
            .toPromise()
            .then(function (response) { return response.json(); })
            .catch(this.handleError);
    };
    DemoHttpService.prototype.adminDestroyAllStructuresForTeamId = function (serverKey, teamId) {
        return Promise.resolve(null);
    };
    DemoHttpService.prototype.adminDestroyStructuresForTeamIdAtPosition = function (serverKey, teamId, x, y, radius, rafts) {
        return Promise.resolve(null);
    };
    DemoHttpService.prototype.adminDestroyDinosForTeamId = function (serverKey, teamId) {
        return Promise.resolve(null);
    };
    DemoHttpService.prototype.adminSaveWorld = function (serverKey) {
        return Promise.resolve(null);
    };
    return DemoHttpService;
}(__WEBPACK_IMPORTED_MODULE_3__http_service__["a" /* HttpService */]));
DemoHttpService = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["d" /* Injectable */])(),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__angular_http__["b" /* Http */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_http__["b" /* Http */]) === "function" && _a || Object])
], DemoHttpService);

var _a;
//# sourceMappingURL=demo.http.service.js.map

/***/ }),

/***/ 309:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__message_service__ = __webpack_require__(19);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return DeveloperComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var DeveloperComponent = (function () {
    function DeveloperComponent(dataService, messageService, notificationsService) {
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.menuOption = undefined;
        this.demoMode = false;
    }
    DeveloperComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) { return _this.showServerUpdateNotification(serverKey); });
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) { return _this.menuOption = menuOption; });
        this.demoMode = localStorage.getItem('demoMode') == 'true';
    };
    DeveloperComponent.prototype.ngOnDestroy = function () {
        this.serverUpdatedSubscription.unsubscribe();
        this.menuOptionSubscription.unsubscribe();
    };
    DeveloperComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    DeveloperComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    DeveloperComponent.prototype.toggleDemoMode = function () {
        var demoMode = localStorage.getItem('demoMode') != 'true';
        this.demoMode = demoMode;
        localStorage.setItem('demoMode', demoMode + '');
    };
    return DeveloperComponent;
}());
DeveloperComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-developer',
        template: __webpack_require__(406),
        styles: [__webpack_require__(384)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _c || Object])
], DeveloperComponent);

var _a, _b, _c;
//# sourceMappingURL=developer.component.js.map

/***/ }),

/***/ 310:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__ = __webpack_require__(39);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return PlayerMenuComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var PlayerMenuComponent = (function () {
    function PlayerMenuComponent(route, dataService) {
        this.route = route;
        this.dataService = dataService;
    }
    PlayerMenuComponent.prototype.ngOnInit = function () {
        this.steamId = this.route.snapshot.params['playerid'];
        if (this.dataService.hasFeatureAccess('player', 'profile', this.steamId))
            this.menu.activate("profile");
        else if (this.dataService.hasFeatureAccess('player', 'creatures', this.steamId))
            this.menu.activate("creatures");
        else if (this.dataService.hasFeatureAccess('player', 'creatures-cloud', this.steamId))
            this.menu.activate("creatures_cloud");
        else if (this.dataService.hasFeatureAccess('player', 'breeding', this.steamId))
            this.menu.activate("breeding");
        else if (this.dataService.hasFeatureAccess('player', 'crops', this.steamId))
            this.menu.activate("crop_plots");
        else if (this.dataService.hasFeatureAccess('player', 'generators', this.steamId))
            this.menu.activate("electrical_generators");
        else if (this.dataService.hasFeatureAccess('player', 'kibbles-eggs', this.steamId))
            this.menu.activate("kibbles_and_eggs");
        else if (this.dataService.hasFeatureAccess('player', 'tribelog', this.steamId))
            this.menu.activate("tribelog");
    };
    return PlayerMenuComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('menu'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */]) === "function" && _a || Object)
], PlayerMenuComponent.prototype, "menu", void 0);
PlayerMenuComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-player-menu',
        host: { '[class]': 'menu.className' },
        template: __webpack_require__(408),
        styles: [__webpack_require__(386)]
    }),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_2__angular_router__["g" /* ActivatedRoute */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__angular_router__["g" /* ActivatedRoute */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */]) === "function" && _c || Object])
], PlayerMenuComponent);

var _a, _b, _c;
//# sourceMappingURL=player-menu.component.js.map

/***/ }),

/***/ 311:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__message_service__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__utils__ = __webpack_require__(107);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return PlayerComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







var PlayerComponent = (function () {
    function PlayerComponent(route, router, httpService, dataService, messageService, notificationsService, ref) {
        this.route = route;
        this.router = router;
        this.httpService = httpService;
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.ref = ref;
        this.menuOption = undefined;
        this.theme = undefined;
        this.imprintNotifications = false;
        this.keysGetter = Object.keys;
        this.loaded = false;
        this.showMap = false;
        this.creaturesMode = "status";
        this.creatureStates = {};
        this.creaturesSortField = "food";
        this.creaturesAltSortFields = "name";
        this.creaturesSortFunctions = {
            "food": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["c" /* floatCompare */])(o1.FoodStatus, o2.FoodStatus, asc, 2); },
            "name": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["a" /* stringLocaleCompare */])(o1.Name, o2.Name, asc); },
            "species": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["a" /* stringLocaleCompare */])(o1.Species, o2.Species, asc); },
            "gender": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["a" /* stringLocaleCompare */])(o1.Gender, o2.Gender, asc); },
            "base_level": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseLevel, o2.BaseLevel, !asc); },
            "level": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.Level == o1.BaseLevel ? null : o1.Level, o2.Level == o2.BaseLevel ? null : o2.Level, !asc); },
            "imprint": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["c" /* floatCompare */])(o1.Imprint, o2.Imprint, !asc, 2); },
            "latitude": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["c" /* floatCompare */])(o1.Latitude, o2.Latitude, asc, 1); },
            "longitude": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["c" /* floatCompare */])(o1.Longitude, o2.Longitude, asc, 1); },
            "owner": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["a" /* stringLocaleCompare */])(o1.OwnerType, o2.OwnerType, asc); },
            "stat_health": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Health : null, o2.BaseStats != undefined ? o2.BaseStats.Health : null, !asc); },
            "stat_stamina": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Stamina : null, o2.BaseStats != undefined ? o2.BaseStats.Stamina : null, !asc); },
            "stat_oxygen": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Oxygen : null, o2.BaseStats != undefined ? o2.BaseStats.Oxygen : null, !asc); },
            "stat_food": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Food : null, o2.BaseStats != undefined ? o2.BaseStats.Food : null, !asc); },
            "stat_weight": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Weight : null, o2.BaseStats != undefined ? o2.BaseStats.Weight : null, !asc); },
            "stat_melee": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Melee : null, o2.BaseStats != undefined ? o2.BaseStats.Melee : null, !asc); },
            "stat_speed": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.MovementSpeed : null, o2.BaseStats != undefined ? o2.BaseStats.MovementSpeed : null, !asc); },
            "id1": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.Id1, o2.Id1, asc); },
            "id2": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_6__utils__["b" /* intCompare */])(o1.Id1, o2.Id1, asc); }
        };
        this.tribeLogFilterFunction = function (o1, filter) { return filter == null ? true : o1.Message != null && o1.Message.toLowerCase().indexOf(filter) >= 0; };
    }
    PlayerComponent.prototype.getPlayer = function () {
        var _this = this;
        this.httpService
            .getPlayer(this.steamId)
            .then(function (player) {
            var serverKeys = Object.keys(player.Servers);
            if (!_this.serverKey || serverKeys.find(function (k) { return k == _this.serverKey; }) == undefined)
                _this.serverKey = serverKeys.length > 0 ? serverKeys[0] : null;
            var clusterKeys = Object.keys(player.Clusters);
            if (!_this.clusterKey || clusterKeys.find(function (k) { return k == _this.clusterKey; }) == undefined)
                _this.clusterKey = clusterKeys.length > 0 ? clusterKeys[0] : null;
            _this.player = player;
            _this.filterAndSort();
            _this.sortCluster();
            _this.filterCluster();
            _this.loaded = true;
            _this.ref.detectChanges(); //todo: evaluate
        })
            .catch(function (error) {
            _this.player = null;
            _this.filteredCreatures = null;
            _this.imprintCreatures = null;
            _this.filteredClusterCreatures = null;
            _this.loaded = true;
        });
    };
    PlayerComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) { return _this.menuOption = menuOption; });
        this.theme$ = this.dataService.Theme;
        this.themeSubscription = this.theme$.subscribe(function (theme) { _this.theme = theme; });
        this.steamId = this.route.snapshot.params['playerid'];
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) { return _this.updateServer(serverKey); });
        this.getPlayer();
    };
    PlayerComponent.prototype.ngOnDestroy = function () {
        this.menuOptionSubscription.unsubscribe();
        this.themeSubscription.unsubscribe();
        this.serverUpdatedSubscription.unsubscribe();
    };
    PlayerComponent.prototype.haveMatingCooldown = function (creature) {
        return creature.NextMating != null ? new Date(creature.NextMating) > new Date() : false;
    };
    PlayerComponent.prototype.active = function (serverKey) {
        return this.serverKey == serverKey;
    };
    PlayerComponent.prototype.activate = function (serverKey) {
        this.serverKey = serverKey;
        this.filterAndSort();
    };
    PlayerComponent.prototype.serverWidth = function () {
        var len = Object.keys(this.player.Servers).length;
        return 100.0 / len;
    };
    PlayerComponent.prototype.activeCluster = function (clusterKey) {
        return this.clusterKey == clusterKey;
    };
    PlayerComponent.prototype.activateCluster = function (clusterKey) {
        this.clusterKey = clusterKey;
        this.sortCluster();
        this.filterCluster();
    };
    PlayerComponent.prototype.clusterWidth = function () {
        var len = Object.keys(this.player.Clusters).length;
        return 100.0 / len;
    };
    PlayerComponent.prototype.sort = function () {
        var _this = this;
        var asc = this.creaturesSortField[0] != '-';
        var sortFunc = this.creaturesSortFunctions[this.creaturesSortField.replace(/^\-/, "")];
        var alts = this.creaturesAltSortFields.split(',').map(function (k) {
            var a = {};
            a.asc = k[0] != '-';
            a.sortFunc = _this.creaturesSortFunctions[k.replace(/^\-/, "")];
            return a;
        });
        this.filteredCreatures.sort(function (o1, o2) {
            var r = sortFunc(o1, o2, asc);
            if (r == 0) {
                for (var _i = 0, alts_1 = alts; _i < alts_1.length; _i++) {
                    var alt = alts_1[_i];
                    r = alt.sortFunc(o1, o2, alt.asc);
                    if (r != 0)
                        break;
                }
            }
            return r;
        });
    };
    PlayerComponent.prototype.filter = function () {
        if (this.creaturesFilter == null || this.creaturesFilter.length == 0)
            this.filteredCreatures = this.player.Servers[this.serverKey].Creatures;
        else {
            var filter_1 = this.creaturesFilter.toLowerCase();
            this.filteredCreatures = this.player.Servers[this.serverKey].Creatures.filter(function (creature) {
                return (creature.Species != null && creature.Species.toLowerCase().indexOf(filter_1) >= 0)
                    || (creature.Name != null && creature.Name.toLowerCase().indexOf(filter_1) >= 0);
            });
        }
        var imprintCreatures = this.player.Servers[this.serverKey].Creatures.filter(function (creature) { return creature.BabyAge != null; });
        imprintCreatures.sort(function (c1, c2) {
            if (new Date(c1.BabyNextCuddle) < new Date(c2.BabyNextCuddle)) {
                return -1;
            }
            else if (new Date(c1.BabyNextCuddle) > new Date(c2.BabyNextCuddle)) {
                return 1;
            }
            else {
                return 0;
            }
        });
        this.imprintCreatures = imprintCreatures;
        var points = [];
        for (var _i = 0, _a = this.filteredCreatures; _i < _a.length; _i++) {
            var creature = _a[_i];
            var point = {};
            point.x = creature.TopoMapX;
            point.y = creature.TopoMapY;
            points.push(point);
        }
        this.points = points;
    };
    PlayerComponent.prototype.filterAndSort = function () {
        this.filter();
        this.sort();
    };
    PlayerComponent.prototype.sortCluster = function () {
        if (this.clusterKey == null)
            return;
        this.player.Clusters[this.clusterKey].Creatures.sort(function (c1, c2) {
            if (c1.Level > c2.Level) {
                return -1;
            }
            else if (c1.Level < c2.Level) {
                return 1;
            }
            else {
                return 0;
            }
        });
    };
    PlayerComponent.prototype.filterCluster = function () {
        if (this.clusterKey == null) {
            this.filteredClusterCreatures = null;
            return;
        }
        if (this.creaturesClusterFilter == null || this.creaturesClusterFilter.length == 0)
            this.filteredClusterCreatures = this.player.Clusters[this.clusterKey].Creatures;
        else {
            var filter_2 = this.creaturesClusterFilter.toLowerCase();
            this.filteredClusterCreatures = this.player.Clusters[this.clusterKey].Creatures.filter(function (creature) {
                return (creature.Species != null && creature.Species.toLowerCase().indexOf(filter_2) >= 0)
                    || (creature.Name != null && creature.Name.toLowerCase().indexOf(filter_2) >= 0);
            });
        }
    };
    PlayerComponent.prototype.run = function () {
        if (this.steamId == null || this.steamId == "") {
            this.player = null;
            this.filteredCreatures = null;
            this.imprintCreatures = null;
            return;
        }
        this.getPlayer();
    };
    PlayerComponent.prototype.openMap = function (event) {
        this.showMap = true;
        event.stopPropagation();
    };
    PlayerComponent.prototype.closeMap = function (event) {
        this.showMap = false;
    };
    PlayerComponent.prototype.updateServer = function (serverKey) {
        this.getPlayer();
        this.showServerUpdateNotification(serverKey);
    };
    PlayerComponent.prototype.haveCluster = function () {
        return this.player != null && Object.keys(this.player.Clusters).length > 0;
    };
    PlayerComponent.prototype.sumKibbleAndEggs = function () {
        return this.player.Servers[this.serverKey].KibblesAndEggs != undefined ? this.player.Servers[this.serverKey].KibblesAndEggs.reduce(function (a, b) { return a + b.KibbleCount + b.EggCount; }, 0) : 0;
    };
    PlayerComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    PlayerComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    PlayerComponent.prototype.getStateForCreature = function (creature) {
        if (!creature)
            return undefined;
        var s = this.creatureStates[creature.Id1 + "_" + creature.Id2];
        if (!s) {
            s = { imprintNotifications: true };
            this.creatureStates[creature.Id1 + "_" + creature.Id2] = s;
        }
        return s;
    };
    PlayerComponent.prototype.toggleImprintNotificationForCreature = function (creature) {
        var s = this.getStateForCreature(creature);
        s.imprintNotifications = !s.imprintNotifications;
    };
    PlayerComponent.prototype.activeCreaturesMode = function (mode) {
        return mode == this.creaturesMode;
    };
    PlayerComponent.prototype.activateCreaturesMode = function (mode) {
        this.creaturesMode = mode;
    };
    PlayerComponent.prototype.setCreaturesSort = function (field) {
        var reverse = this.creaturesSortField == field;
        if (reverse)
            this.creaturesSortField = "-" + field;
        else
            this.creaturesSortField = field;
        if (field == "latitude")
            this.creaturesAltSortFields = !reverse ? "longitude,name" : "-longitude,name";
        else if (field == "longitude")
            this.creaturesAltSortFields = !reverse ? "latitude,name" : "-latitude,name";
        else
            this.creaturesAltSortFields = "name";
        this.sort();
    };
    PlayerComponent.prototype.copyCreature = function (creature) {
    };
    PlayerComponent.prototype.getCurrentServer = function () {
        var _this = this;
        if (!(this.dataService && this.dataService.Servers && this.dataService.Servers.Servers))
            return undefined;
        var server = this.dataService.Servers.Servers.find(function (s) { return s.Key == _this.serverKey; });
        return server;
    };
    PlayerComponent.prototype.numCreatureTabs = function () {
        var num = 1;
        if (this.dataService.hasFeatureAccess('player', 'creatures-basestats', this.steamId))
            num += 1;
        if (this.dataService.hasFeatureAccess('player', 'creatures-ids', this.steamId))
            num += 1;
        return num;
    };
    PlayerComponent.prototype.isTheme = function (theme) {
        return this.theme == theme;
    };
    return PlayerComponent;
}());
PlayerComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-player',
        template: __webpack_require__(409),
        styles: [__webpack_require__(387)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["g" /* ActivatedRoute */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["g" /* ActivatedRoute */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_5__http_service__["a" /* HttpService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_5__http_service__["a" /* HttpService */]) === "function" && _c || Object, typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__data_service__["a" /* DataService */]) === "function" && _d || Object, typeof (_e = typeof __WEBPACK_IMPORTED_MODULE_4__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_4__message_service__["a" /* MessageService */]) === "function" && _e || Object, typeof (_f = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _f || Object, typeof (_g = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */]) === "function" && _g || Object])
], PlayerComponent);

var _a, _b, _c, _d, _e, _f, _g;
//# sourceMappingURL=player.component.js.map

/***/ }),

/***/ 312:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__ = __webpack_require__(35);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_moment__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_moment___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_moment__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return RelativeTimeComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};



var RelativeTimeComponent = (function () {
    function RelativeTimeComponent(ref) {
        this.ref = ref;
        this._time = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
    }
    Object.defineProperty(RelativeTimeComponent.prototype, "time", {
        get: function () {
            return this._time.getValue();
        },
        set: function (value) {
            this._time.next(value);
        },
        enumerable: true,
        configurable: true
    });
    ;
    RelativeTimeComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._timeSubscription = this._time.subscribe(function (value) {
            _this.update();
        });
        this._counter = __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["Observable"].interval(1000).map(function (x) { return x; });
        this._counterSubscription = this._counter.subscribe(function (x) { return _this.update(); });
    };
    RelativeTimeComponent.prototype.ngOnDestroy = function () {
        this._timeSubscription.unsubscribe();
        this._counterSubscription.unsubscribe();
    };
    RelativeTimeComponent.prototype.update = function () {
        var newValue = this.toRelativeDate(this.time);
        var oldValue = this._str;
        if (newValue != oldValue) {
            this._str = newValue;
            this.ref.markForCheck();
        }
    };
    RelativeTimeComponent.prototype.toRelativeDate = function (datejson) {
        return __WEBPACK_IMPORTED_MODULE_2_moment__(new Date(datejson)).fromNow();
    };
    return RelativeTimeComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object),
    __metadata("design:paramtypes", [Object])
], RelativeTimeComponent.prototype, "time", null);
RelativeTimeComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'relative-time',
        template: "<span>{{_str}}</span>",
        styles: [__webpack_require__(388)],
        changeDetection: __WEBPACK_IMPORTED_MODULE_0__angular_core__["_13" /* ChangeDetectionStrategy */].OnPush
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */]) === "function" && _a || Object])
], RelativeTimeComponent);

var _a;
//# sourceMappingURL=relative-time.component.js.map

/***/ }),

/***/ 313:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__ = __webpack_require__(18);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return SanitizeHtmlPipe; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var SanitizeHtmlPipe = (function () {
    function SanitizeHtmlPipe(_sanitizer) {
        this._sanitizer = _sanitizer;
    }
    SanitizeHtmlPipe.prototype.transform = function (html) {
        return this._sanitizer.bypassSecurityTrustHtml(html);
    };
    return SanitizeHtmlPipe;
}());
SanitizeHtmlPipe = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["S" /* Pipe */])({
        name: 'sanitizeHtml'
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__["c" /* DomSanitizer */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__["c" /* DomSanitizer */]) === "function" && _a || Object])
], SanitizeHtmlPipe);

var _a;
//# sourceMappingURL=sanitize-html.pipe.js.map

/***/ }),

/***/ 314:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__ = __webpack_require__(18);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return SanitizeStylePipe; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var SanitizeStylePipe = (function () {
    function SanitizeStylePipe(_sanitizer) {
        this._sanitizer = _sanitizer;
    }
    SanitizeStylePipe.prototype.transform = function (style) {
        return this._sanitizer.bypassSecurityTrustStyle(style);
    };
    return SanitizeStylePipe;
}());
SanitizeStylePipe = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["S" /* Pipe */])({
        name: 'sanitizeStyle'
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__["c" /* DomSanitizer */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_platform_browser__["c" /* DomSanitizer */]) === "function" && _a || Object])
], SanitizeStylePipe);

var _a;
//# sourceMappingURL=sanitize-style.pipe.js.map

/***/ }),

/***/ 315:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__ = __webpack_require__(39);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ServerListMenuComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};



var ServerListMenuComponent = (function () {
    function ServerListMenuComponent(dataService) {
        this.dataService = dataService;
    }
    ServerListMenuComponent.prototype.ngOnInit = function () {
        this.menu.activate("overview");
    };
    return ServerListMenuComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('menu'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */]) === "function" && _a || Object)
], ServerListMenuComponent.prototype, "menu", void 0);
ServerListMenuComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-server-list-menu',
        host: { '[class]': 'menu.className' },
        template: __webpack_require__(410),
        styles: [__webpack_require__(389)]
    }),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */]) === "function" && _b || Object])
], ServerListMenuComponent);

var _a, _b;
//# sourceMappingURL=server-list-menu.component.js.map

/***/ }),

/***/ 316:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3__message_service__ = __webpack_require__(19);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ServerListComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};




var ServerListComponent = (function () {
    function ServerListComponent(dataService, messageService, notificationsService) {
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.menuOption = undefined;
        this.serverCount = 0;
        this.onlinePlayerCount = 0;
    }
    ServerListComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) { return _this.showServerUpdateNotification(serverKey); });
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) { return _this.menuOption = menuOption; });
        this.serversUpdatedSubscription = this.dataService.ServersUpdated$.subscribe(function (servers) {
            _this.updateData(servers);
        });
        this.serverUpdateInterval = window.setInterval(function () {
            _this.dataService.updateServer(null);
        }, 60000);
        //init aggregated data
        this.updateData(this.dataService.Servers);
    };
    ServerListComponent.prototype.ngOnDestroy = function () {
        this.serverUpdatedSubscription.unsubscribe();
        this.menuOptionSubscription.unsubscribe();
        this.serversUpdatedSubscription.unsubscribe();
        window.clearInterval(this.serverUpdateInterval);
    };
    ServerListComponent.prototype.updateData = function (servers) {
        var serverCount = 0;
        var onlinePlayerCount = 0;
        if (servers && servers.Servers) {
            serverCount = servers.Servers.length;
            for (var _i = 0, _a = servers.Servers; _i < _a.length; _i++) {
                var server = _a[_i];
                if (!server.OnlinePlayers)
                    continue;
                onlinePlayerCount += server.OnlinePlayers.length;
            }
        }
        this.serverCount = serverCount;
        this.onlinePlayerCount = onlinePlayerCount;
    };
    ServerListComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    ServerListComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    return ServerListComponent;
}());
ServerListComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-server-list',
        template: __webpack_require__(411),
        styles: [__webpack_require__(390)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_3__message_service__["a" /* MessageService */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _c || Object])
], ServerListComponent);

var _a, _b, _c;
//# sourceMappingURL=server-list.component.js.map

/***/ }),

/***/ 317:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__ = __webpack_require__(39);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ServerMenuComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};



var ServerMenuComponent = (function () {
    function ServerMenuComponent(dataService) {
        this.dataService = dataService;
    }
    ServerMenuComponent.prototype.ngOnInit = function () {
        if (this.dataService.hasFeatureAccess('server', 'players'))
            this.menu.activate("players");
        else if (this.dataService.hasFeatureAccess('server', 'tribes'))
            this.menu.activate("tribes");
        else if (this.dataService.hasFeatureAccess('server', 'wildcreatures-statistics'))
            this.menu.activate("wildcreatures-statistics");
        else if (this.dataService.hasFeatureAccess('server', 'wildcreatures'))
            this.menu.activate("wildcreatures");
    };
    return ServerMenuComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_17" /* ViewChild */])('menu'),
    __metadata("design:type", typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__menu_menu_component__["a" /* MenuComponent */]) === "function" && _a || Object)
], ServerMenuComponent.prototype, "menu", void 0);
ServerMenuComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-server-menu',
        host: { '[class]': 'menu.className' },
        template: __webpack_require__(412),
        styles: [__webpack_require__(391)]
    }),
    __metadata("design:paramtypes", [typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2__data_service__["a" /* DataService */]) === "function" && _b || Object])
], ServerMenuComponent);

var _a, _b;
//# sourceMappingURL=server-menu.component.js.map

/***/ }),

/***/ 318:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__angular_router__ = __webpack_require__(21);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__ = __webpack_require__(23);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_moment__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_moment___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_3_moment__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_4__data_service__ = __webpack_require__(10);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_5__message_service__ = __webpack_require__(19);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_6__http_service__ = __webpack_require__(29);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_7__utils__ = __webpack_require__(107);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return ServerComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};








var ServerComponent = (function () {
    function ServerComponent(route, router, httpService, dataService, messageService, notificationsService, ref) {
        this.route = route;
        this.router = router;
        this.httpService = httpService;
        this.dataService = dataService;
        this.messageService = messageService;
        this.notificationsService = notificationsService;
        this.ref = ref;
        this.menuOption = undefined;
        this.loaded = false;
        this.creaturesLoaded = false;
        this.keysGetter = Object.keys;
        this.showMap = false;
        this.creaturesMode = "status";
        this.creaturesSortField = "base_level";
        this.creaturesAltSortFields = "base_level,gender";
        this.creaturesSortFunctions = {
            "gender": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(o1.Gender, o2.Gender, asc); },
            "base_level": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseLevel, o2.BaseLevel, !asc); },
            "tameable": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.IsTameable, o2.IsTameable, !asc); },
            "latitude": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["c" /* floatCompare */])(o1.Latitude, o2.Latitude, asc, 1); },
            "longitude": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["c" /* floatCompare */])(o1.Longitude, o2.Longitude, asc, 1); },
            "x": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["c" /* floatCompare */])(o1.X, o2.X, asc, 0); },
            "y": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["c" /* floatCompare */])(o1.Y, o2.Y, asc, 0); },
            "z": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["c" /* floatCompare */])(o1.Z, o2.Z, asc, 0); },
            "stat_health": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Health : null, o2.BaseStats != undefined ? o2.BaseStats.Health : null, !asc); },
            "stat_stamina": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Stamina : null, o2.BaseStats != undefined ? o2.BaseStats.Stamina : null, !asc); },
            "stat_oxygen": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Oxygen : null, o2.BaseStats != undefined ? o2.BaseStats.Oxygen : null, !asc); },
            "stat_food": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Food : null, o2.BaseStats != undefined ? o2.BaseStats.Food : null, !asc); },
            "stat_weight": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Weight : null, o2.BaseStats != undefined ? o2.BaseStats.Weight : null, !asc); },
            "stat_melee": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.Melee : null, o2.BaseStats != undefined ? o2.BaseStats.Melee : null, !asc); },
            "stat_speed": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.BaseStats != undefined ? o1.BaseStats.MovementSpeed : null, o2.BaseStats != undefined ? o2.BaseStats.MovementSpeed : null, !asc); },
            "id1": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.Id1, o2.Id1, asc); },
            "id2": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.Id1, o2.Id1, asc); }
        };
        this.playerSortFunctions = {
            "character_name": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(o1.CharacterName, o2.CharacterName, asc); },
            "tribe_name": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(o1.TribeName, o2.TribeName, asc); },
            "last_active": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.LastActiveTime, o2.LastActiveTime, !asc); }
        };
        this.tribeSortFunctions = {
            "tribe_name": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(o1.Name, o2.Name, asc); },
            "last_active": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.LastActiveTime, o2.LastActiveTime, !asc); }
        };
        this.wildStatisticsSortFunctions = {
            "species": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(o1.Name, o2.Name, asc); },
            "class_name": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(o1.ClassName, o2.ClassName, asc); },
            "count": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["b" /* intCompare */])(o1.Count, o2.Count, !asc); },
            "fraction": function (o1, o2, asc) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["c" /* floatCompare */])(o1.Fraction, o2.Fraction, !asc, 4); },
        };
    }
    ServerComponent.prototype.getServer = function () {
        var _this = this;
        this.httpService
            .getServer(this.serverKey)
            .then(function (server) {
            _this.server = server;
            _this.filter();
            _this.loaded = true;
        })
            .catch(function (error) {
            _this.server = null;
            _this.filteredPlayers = null;
            _this.filteredTribes = null;
            _this.loaded = true;
        });
    };
    ServerComponent.prototype.getWildCreatures = function () {
        var _this = this;
        this.httpService
            .getWildCreatures(this.serverKey)
            .then(function (wild) {
            _this.wild = wild;
            _this.species = Object.keys(_this.wild.Species).sort(function (a, b) { return __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_7__utils__["a" /* stringLocaleCompare */])(_this.wild.Species[a].Name || a, _this.wild.Species[b].Name || b, true); });
            if (!_this.selectedSpecies || _this.species.find(function (k) { return k == _this.selectedSpecies; }) == undefined)
                _this.selectedSpecies = _this.species.length > 0 ? _this.species[0] : null;
            _this.filterAndSortWild();
            _this.creaturesLoaded = true;
            _this.ref.detectChanges(); //todo: evaluate
        })
            .catch(function (error) {
            _this.wild = null;
            _this.species = null;
            _this.filteredCreatures = null;
            _this.creaturesLoaded = true;
        });
    };
    ServerComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.accessControl_pages_player = this.dataService.hasFeatureAccessObservable('pages', 'player');
        this.serverKey = this.route.snapshot.params['id'];
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) {
            _this.menuOption = menuOption;
            //todo: should it be possible to reload this data?
            if (_this.creaturesLoaded == false && (_this.menuOption == "wildcreatures" || _this.menuOption == "wildcreatures-statistics")) {
                _this.getWildCreatures();
            }
        });
        this.serverUpdatedSubscription = this.messageService.serverUpdated$.subscribe(function (serverKey) {
            if (_this.serverKey == serverKey) {
                _this.updateServer();
                _this.showServerUpdateNotification(serverKey);
            }
        });
        this.getServer();
    };
    ServerComponent.prototype.ngOnDestroy = function () {
        this.menuOptionSubscription.unsubscribe();
        this.serverUpdatedSubscription.unsubscribe();
    };
    ServerComponent.prototype.filter = function () {
        var currentDate = this.dataService.getCurrentDate();
        var pastDate = currentDate.subtract(90, 'day');
        this.filteredPlayers = this.server.Players.filter(function (player) {
            return __WEBPACK_IMPORTED_MODULE_3_moment__(new Date(player.LastActiveTime)).isSameOrAfter(pastDate);
        });
        this.filteredTribes = this.server.Tribes.filter(function (tribe) {
            return __WEBPACK_IMPORTED_MODULE_3_moment__(new Date(tribe.LastActiveTime)).isSameOrAfter(pastDate);
        });
    };
    ServerComponent.prototype.sortWild = function () {
        var _this = this;
        var asc = this.creaturesSortField[0] != '-';
        var sortFunc = this.creaturesSortFunctions[this.creaturesSortField.replace(/^\-/, "")];
        var alts = this.creaturesAltSortFields.split(',').map(function (k) {
            var a = {};
            a.asc = k[0] != '-';
            a.sortFunc = _this.creaturesSortFunctions[k.replace(/^\-/, "")];
            return a;
        });
        if (this.filteredCreatures != undefined) {
            this.filteredCreatures.sort(function (o1, o2) {
                var r = sortFunc(o1, o2, asc);
                if (r == 0) {
                    for (var _i = 0, alts_1 = alts; _i < alts_1.length; _i++) {
                        var alt = alts_1[_i];
                        r = alt.sortFunc(o1, o2, alt.asc);
                        if (r != 0)
                            break;
                    }
                }
                return r;
            });
        }
    };
    ServerComponent.prototype.filterWild = function () {
        if (!this.selectedSpecies)
            this.filteredCreatures = undefined;
        else
            this.filteredCreatures = this.wild.Species[this.selectedSpecies].Creatures;
        /*if (this.creaturesFilter == null || this.creaturesFilter.length == 0) this.filteredCreatures = this.wild.Creatures;
        else {
          let filter = this.creaturesFilter.toLowerCase();
          this.filteredCreatures = this.wild.Creatures.filter(creature =>
            (creature.Species != null && creature.Species.toLowerCase().indexOf(filter) >= 0));
        }*/
        var points = [];
        if (this.filteredCreatures != undefined) {
            for (var _i = 0, _a = this.filteredCreatures; _i < _a.length; _i++) {
                var creature = _a[_i];
                var point = {};
                point.x = creature.TopoMapX;
                point.y = creature.TopoMapY;
                points.push(point);
            }
        }
        this.points = points;
    };
    ServerComponent.prototype.filterAndSortWild = function () {
        this.filterWild();
        this.sortWild();
    };
    ServerComponent.prototype.openMap = function (event) {
        this.showMap = true;
        event.stopPropagation();
    };
    ServerComponent.prototype.closeMap = function (event) {
        this.showMap = false;
    };
    ServerComponent.prototype.activeCreaturesMode = function (mode) {
        return mode == this.creaturesMode;
    };
    ServerComponent.prototype.activateCreaturesMode = function (mode) {
        this.creaturesMode = mode;
    };
    ServerComponent.prototype.setCreaturesSort = function (field) {
        var reverse = this.creaturesSortField == field;
        if (reverse)
            this.creaturesSortField = "-" + field;
        else
            this.creaturesSortField = field;
        if (field == "latitude")
            this.creaturesAltSortFields = !reverse ? "longitude" : "-longitude";
        else if (field == "longitude")
            this.creaturesAltSortFields = !reverse ? "latitude" : "-latitude";
        else
            this.creaturesAltSortFields = "base_level,gender";
        this.sortWild();
    };
    ServerComponent.prototype.numCreatureTabs = function () {
        var num = 1;
        if (this.dataService.hasFeatureAccess('server', 'wildcreatures-basestats'))
            num += 1;
        if (this.dataService.hasFeatureAccess('server', 'wildcreatures-ids'))
            num += 1;
        return num;
    };
    ServerComponent.prototype.toRelativeDate = function (datejson) {
        return __WEBPACK_IMPORTED_MODULE_3_moment__(new Date(datejson)).fromNow();
    };
    ServerComponent.prototype.getTribeMember = function (steamId) {
        return this.server.Players.find(function (p) { return p.SteamId == steamId; });
    };
    ServerComponent.prototype.updateServer = function () {
        this.getServer();
    };
    ServerComponent.prototype.showServerUpdateNotification = function (serverKey) {
        this.notificationsService.success('Server Update', serverKey + " was updated; Reloading data...", {
            showProgressBar: true,
            pauseOnHover: true,
            clickToClose: true
        });
    };
    ServerComponent.prototype.isMenuActive = function (menuOption) {
        return this.menuOption == menuOption;
    };
    return ServerComponent;
}());
ServerComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-server',
        template: __webpack_require__(413),
        styles: [__webpack_require__(392)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["g" /* ActivatedRoute */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["g" /* ActivatedRoute */]) === "function" && _a || Object, typeof (_b = typeof __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__angular_router__["b" /* Router */]) === "function" && _b || Object, typeof (_c = typeof __WEBPACK_IMPORTED_MODULE_6__http_service__["a" /* HttpService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_6__http_service__["a" /* HttpService */]) === "function" && _c || Object, typeof (_d = typeof __WEBPACK_IMPORTED_MODULE_4__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_4__data_service__["a" /* DataService */]) === "function" && _d || Object, typeof (_e = typeof __WEBPACK_IMPORTED_MODULE_5__message_service__["a" /* MessageService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_5__message_service__["a" /* MessageService */]) === "function" && _e || Object, typeof (_f = typeof __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_2_angular2_notifications__["b" /* NotificationsService */]) === "function" && _f || Object, typeof (_g = typeof __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_0__angular_core__["r" /* ChangeDetectorRef */]) === "function" && _g || Object])
], ServerComponent);

var _a, _b, _c, _d, _e, _f, _g;
//# sourceMappingURL=server.component.js.map

/***/ }),

/***/ 319:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__ = __webpack_require__(35);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2__environments_environment__ = __webpack_require__(22);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_moment__ = __webpack_require__(1);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_3_moment___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_3_moment__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return TimerComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};





var TimerComponent = (function () {
    function TimerComponent() {
        this._ready = false;
        //private _completed: boolean = false;
        this._wasExpired = false;
        this._notificationSent = false;
        this._time = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
        this._notification = new __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["BehaviorSubject"](undefined);
        this._loadedAt = __WEBPACK_IMPORTED_MODULE_3_moment__();
    }
    Object.defineProperty(TimerComponent.prototype, "time", {
        get: function () {
            return this._time.getValue();
        },
        set: function (value) {
            this._time.next(value);
        },
        enumerable: true,
        configurable: true
    });
    ;
    Object.defineProperty(TimerComponent.prototype, "notification", {
        get: function () {
            return this._notification.getValue();
        },
        set: function (value) {
            this._notification.next(value);
        },
        enumerable: true,
        configurable: true
    });
    ;
    TimerComponent.prototype.updateDiff = function (initialTime) {
        //console.log("initialTime: " + initialTime);
        if (initialTime) {
            if (!__WEBPACK_IMPORTED_MODULE_2__environments_environment__["a" /* environment */].demo)
                this._wasExpired = __WEBPACK_IMPORTED_MODULE_3_moment__(new Date(initialTime)).diff(__WEBPACK_IMPORTED_MODULE_3_moment__()) <= 0;
            else
                this._wasExpired = __WEBPACK_IMPORTED_MODULE_3_moment__(new Date(initialTime)).diff(__WEBPACK_IMPORTED_MODULE_3_moment__(new Date(__WEBPACK_IMPORTED_MODULE_2__environments_environment__["a" /* environment */].demoDate))) - __WEBPACK_IMPORTED_MODULE_3_moment__().diff(this._loadedAt) <= 0;
            this._notificationSent = false;
            this._str = undefined;
            this._ready = this._wasExpired;
            if (!this._wasExpired && this.state._completed == true)
                this.state._completed = false;
        }
        if (!__WEBPACK_IMPORTED_MODULE_2__environments_environment__["a" /* environment */].demo)
            this._diff = initialTime || this.time ? __WEBPACK_IMPORTED_MODULE_3_moment__["duration"](__WEBPACK_IMPORTED_MODULE_3_moment__(new Date(initialTime || this.time)).diff(__WEBPACK_IMPORTED_MODULE_3_moment__())) : undefined;
        else
            this._diff = initialTime || this.time ? __WEBPACK_IMPORTED_MODULE_3_moment__["duration"](__WEBPACK_IMPORTED_MODULE_3_moment__(new Date(initialTime || this.time)).diff(__WEBPACK_IMPORTED_MODULE_3_moment__(new Date(__WEBPACK_IMPORTED_MODULE_2__environments_environment__["a" /* environment */].demoDate))) - __WEBPACK_IMPORTED_MODULE_3_moment__().diff(this._loadedAt)) : undefined;
    };
    TimerComponent.prototype.update = function () {
        //console.log("diff: " + this._diff + ", notificationSent: " + this._notificationSent + ", notification: " + this.notification + ", wasExpired: " + this._wasExpired);
        if (!this._diff)
            return "";
        if (this._diff.asMilliseconds() <= 0) {
            if (!this._notificationSent) {
                if (this.notification && this.state.imprintNotifications && !this._wasExpired) {
                    var audio = new Audio('assets/Alarm01.mp3');
                    audio.play();
                }
                this._ready = true;
                //this.state._completed = false;
            }
            this._notificationSent = true;
            this._str = undefined;
            return;
        }
        var seconds = this._diff.seconds();
        var minutes = this._diff.minutes();
        var hours = this._diff.hours();
        var days = Math.floor(this._diff.asDays());
        var components = [];
        if (days > 0)
            components.push(days + 'd');
        if (days > 0 || hours > 0)
            components.push(hours + 'h');
        if (days > 0 || hours > 0 || minutes > 0)
            components.push(minutes + 'm');
        components.push(seconds + 's');
        this._str = components.join(' ');
        this._ready = false;
        this.state._completed = false;
    };
    TimerComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._timeSubscription = this._time.subscribe(function (value) {
            _this.updateDiff(value);
            _this.update();
        });
        this._notificationSubscription = this._notification.subscribe(function (value) {
        });
        this._counter = __WEBPACK_IMPORTED_MODULE_1_rxjs_Rx__["Observable"].interval(1000).map(function (x) {
            _this.updateDiff(undefined);
            return x;
        });
        this._counterSubscription = this._counter.subscribe(function (x) { return _this.update(); });
    };
    TimerComponent.prototype.ngOnDestroy = function () {
        this._timeSubscription.unsubscribe();
        this._notificationSubscription.unsubscribe();
        this._counterSubscription.unsubscribe();
    };
    return TimerComponent;
}());
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object)
], TimerComponent.prototype, "state", void 0);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object),
    __metadata("design:paramtypes", [Object])
], TimerComponent.prototype, "time", null);
__decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["o" /* Input */])(),
    __metadata("design:type", Object),
    __metadata("design:paramtypes", [Object])
], TimerComponent.prototype, "notification", null);
TimerComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'timer',
        template: "<span *ngIf=\"!_ready\">{{_str}}</span><button *ngIf=\"_ready\" style=\"padding: 4px 8px;\" class=\"w3-button w3-small\" [ngClass]=\"{'theme-d1': state._completed, 'theme-l2': !state._completed && _ready, 'theme-hover': !state._completed && _ready}\" (click)=\"state._completed = !state._completed\">{{(state._completed ? \"Completed\" : \"Ready\")}}</button>",
        styles: [__webpack_require__(393)]
    }),
    __metadata("design:paramtypes", [])
], TimerComponent);

//# sourceMappingURL=timer.component.js.map

/***/ }),

/***/ 320:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return commonEnvironment; });
var commonEnvironment = {
    configJs: 'var config = {"webapi":{"port":60001},"webapp":{"defaultTheme":"Dark"}};',
};
//# sourceMappingURL=environment.common.js.map

/***/ }),

/***/ 376:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 377:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 378:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 379:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 380:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, ".map canvas { position: absolute; top: 0; left: 0; width: 100%; }\r\n.map svg { position: absolute; top: 0; left: 0; width: 100%; }\r\nrect.overlay { fill: transparent; }\r\n.wrapper { position: relative; }\r\n.wrapper:after {\r\n  padding-top: 100%;\r\n  display: block;\r\n  content: '';\r\n}\r\n.wrapper .buttons {\r\n  position: absolute;\r\n  left: 5px;\r\n  top: 5px;\r\n  opacity: 0.75;\r\n  z-index: 2;\r\n}\r\n/*.contextMenu {\r\n  position: absolute;\r\n  left: 0px;\r\n  top: 0px;\r\n  background-color: #fff;\r\n  color: #000;\r\n  padding: 10px;\r\n  z-index: 20;\r\n  opacity: 0.90;\r\n  display: none;\r\n}*/\r\n.contextMenu {\r\n}", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 381:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 382:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 383:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "tr th.orderBy { cursor: pointer; }\r\na.w3-button.disabled { color: darkgray; }\r\na.w3-button.disabled:hover { color: darkgray !important; background-color: transparent !important; opacity: 1.0 !important; cursor: default; }", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 384:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 385:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 386:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 387:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 388:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 389:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 39:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0__angular_core__ = __webpack_require__(3);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1__data_service__ = __webpack_require__(10);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "a", function() { return MenuComponent; });
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var MenuComponent = (function () {
    function MenuComponent(dataService) {
        this.dataService = dataService;
        this.menuOption = undefined;
        this.menuVisible = false;
        this.className = "menucontainer";
    }
    MenuComponent.prototype.ngOnInit = function () {
        var _this = this;
        //this.activate("overview");
        this.menuOptionSubscription = this.dataService.MenuOption.subscribe(function (menuOption) { return _this.menuOption = menuOption; });
    };
    MenuComponent.prototype.ngOnDestroy = function () {
        this.menuOptionSubscription.unsubscribe();
    };
    MenuComponent.prototype.activate = function (menuOption) {
        this.dataService.SetMenuOption(menuOption);
    };
    MenuComponent.prototype.active = function (menuOption) {
        return this.menuOption == menuOption;
    };
    MenuComponent.prototype.toggleMenu = function () {
        this.menuVisible = !this.menuVisible;
    };
    return MenuComponent;
}());
MenuComponent = __decorate([
    __webpack_require__.i(__WEBPACK_IMPORTED_MODULE_0__angular_core__["_12" /* Component */])({
        selector: 'app-menu',
        template: __webpack_require__(407),
        styles: [__webpack_require__(385)]
    }),
    __metadata("design:paramtypes", [typeof (_a = typeof __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */] !== "undefined" && __WEBPACK_IMPORTED_MODULE_1__data_service__["a" /* DataService */]) === "function" && _a || Object])
], MenuComponent);

var _a;
//# sourceMappingURL=menu.component.js.map

/***/ }),

/***/ 390:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, ".serverdetails th {\r\n    white-space: nowrap;\r\n}\r\n\r\n.serverdetails td {\r\n    width: 99%;\r\n}", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 391:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 392:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 393:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(7)(false);
// imports


// module
exports.push([module.i, "", ""]);

// exports


/*** EXPORTS FROM exports-loader ***/
module.exports = module.exports.toString();

/***/ }),

/***/ 394:
/***/ (function(module, exports, __webpack_require__) {

var map = {
	"./af": 124,
	"./af.js": 124,
	"./ar": 131,
	"./ar-dz": 125,
	"./ar-dz.js": 125,
	"./ar-kw": 126,
	"./ar-kw.js": 126,
	"./ar-ly": 127,
	"./ar-ly.js": 127,
	"./ar-ma": 128,
	"./ar-ma.js": 128,
	"./ar-sa": 129,
	"./ar-sa.js": 129,
	"./ar-tn": 130,
	"./ar-tn.js": 130,
	"./ar.js": 131,
	"./az": 132,
	"./az.js": 132,
	"./be": 133,
	"./be.js": 133,
	"./bg": 134,
	"./bg.js": 134,
	"./bn": 135,
	"./bn.js": 135,
	"./bo": 136,
	"./bo.js": 136,
	"./br": 137,
	"./br.js": 137,
	"./bs": 138,
	"./bs.js": 138,
	"./ca": 139,
	"./ca.js": 139,
	"./cs": 140,
	"./cs.js": 140,
	"./cv": 141,
	"./cv.js": 141,
	"./cy": 142,
	"./cy.js": 142,
	"./da": 143,
	"./da.js": 143,
	"./de": 146,
	"./de-at": 144,
	"./de-at.js": 144,
	"./de-ch": 145,
	"./de-ch.js": 145,
	"./de.js": 146,
	"./dv": 147,
	"./dv.js": 147,
	"./el": 148,
	"./el.js": 148,
	"./en-au": 149,
	"./en-au.js": 149,
	"./en-ca": 150,
	"./en-ca.js": 150,
	"./en-gb": 151,
	"./en-gb.js": 151,
	"./en-ie": 152,
	"./en-ie.js": 152,
	"./en-nz": 153,
	"./en-nz.js": 153,
	"./eo": 154,
	"./eo.js": 154,
	"./es": 156,
	"./es-do": 155,
	"./es-do.js": 155,
	"./es.js": 156,
	"./et": 157,
	"./et.js": 157,
	"./eu": 158,
	"./eu.js": 158,
	"./fa": 159,
	"./fa.js": 159,
	"./fi": 160,
	"./fi.js": 160,
	"./fo": 161,
	"./fo.js": 161,
	"./fr": 164,
	"./fr-ca": 162,
	"./fr-ca.js": 162,
	"./fr-ch": 163,
	"./fr-ch.js": 163,
	"./fr.js": 164,
	"./fy": 165,
	"./fy.js": 165,
	"./gd": 166,
	"./gd.js": 166,
	"./gl": 167,
	"./gl.js": 167,
	"./gom-latn": 168,
	"./gom-latn.js": 168,
	"./he": 169,
	"./he.js": 169,
	"./hi": 170,
	"./hi.js": 170,
	"./hr": 171,
	"./hr.js": 171,
	"./hu": 172,
	"./hu.js": 172,
	"./hy-am": 173,
	"./hy-am.js": 173,
	"./id": 174,
	"./id.js": 174,
	"./is": 175,
	"./is.js": 175,
	"./it": 176,
	"./it.js": 176,
	"./ja": 177,
	"./ja.js": 177,
	"./jv": 178,
	"./jv.js": 178,
	"./ka": 179,
	"./ka.js": 179,
	"./kk": 180,
	"./kk.js": 180,
	"./km": 181,
	"./km.js": 181,
	"./kn": 182,
	"./kn.js": 182,
	"./ko": 183,
	"./ko.js": 183,
	"./ky": 184,
	"./ky.js": 184,
	"./lb": 185,
	"./lb.js": 185,
	"./lo": 186,
	"./lo.js": 186,
	"./lt": 187,
	"./lt.js": 187,
	"./lv": 188,
	"./lv.js": 188,
	"./me": 189,
	"./me.js": 189,
	"./mi": 190,
	"./mi.js": 190,
	"./mk": 191,
	"./mk.js": 191,
	"./ml": 192,
	"./ml.js": 192,
	"./mr": 193,
	"./mr.js": 193,
	"./ms": 195,
	"./ms-my": 194,
	"./ms-my.js": 194,
	"./ms.js": 195,
	"./my": 196,
	"./my.js": 196,
	"./nb": 197,
	"./nb.js": 197,
	"./ne": 198,
	"./ne.js": 198,
	"./nl": 200,
	"./nl-be": 199,
	"./nl-be.js": 199,
	"./nl.js": 200,
	"./nn": 201,
	"./nn.js": 201,
	"./pa-in": 202,
	"./pa-in.js": 202,
	"./pl": 203,
	"./pl.js": 203,
	"./pt": 205,
	"./pt-br": 204,
	"./pt-br.js": 204,
	"./pt.js": 205,
	"./ro": 206,
	"./ro.js": 206,
	"./ru": 207,
	"./ru.js": 207,
	"./sd": 208,
	"./sd.js": 208,
	"./se": 209,
	"./se.js": 209,
	"./si": 210,
	"./si.js": 210,
	"./sk": 211,
	"./sk.js": 211,
	"./sl": 212,
	"./sl.js": 212,
	"./sq": 213,
	"./sq.js": 213,
	"./sr": 215,
	"./sr-cyrl": 214,
	"./sr-cyrl.js": 214,
	"./sr.js": 215,
	"./ss": 216,
	"./ss.js": 216,
	"./sv": 217,
	"./sv.js": 217,
	"./sw": 218,
	"./sw.js": 218,
	"./ta": 219,
	"./ta.js": 219,
	"./te": 220,
	"./te.js": 220,
	"./tet": 221,
	"./tet.js": 221,
	"./th": 222,
	"./th.js": 222,
	"./tl-ph": 223,
	"./tl-ph.js": 223,
	"./tlh": 224,
	"./tlh.js": 224,
	"./tr": 225,
	"./tr.js": 225,
	"./tzl": 226,
	"./tzl.js": 226,
	"./tzm": 228,
	"./tzm-latn": 227,
	"./tzm-latn.js": 227,
	"./tzm.js": 228,
	"./uk": 229,
	"./uk.js": 229,
	"./ur": 230,
	"./ur.js": 230,
	"./uz": 232,
	"./uz-latn": 231,
	"./uz-latn.js": 231,
	"./uz.js": 232,
	"./vi": 233,
	"./vi.js": 233,
	"./x-pseudo": 234,
	"./x-pseudo.js": 234,
	"./yo": 235,
	"./yo.js": 235,
	"./zh-cn": 236,
	"./zh-cn.js": 236,
	"./zh-hk": 237,
	"./zh-hk.js": 237,
	"./zh-tw": 238,
	"./zh-tw.js": 238
};
function webpackContext(req) {
	return __webpack_require__(webpackContextResolve(req));
};
function webpackContextResolve(req) {
	var id = map[req];
	if(!(id + 1)) // check for number
		throw new Error("Cannot find module '" + req + "'.");
	return id;
};
webpackContext.keys = function webpackContextKeys() {
	return Object.keys(map);
};
webpackContext.resolve = webpackContextResolve;
module.exports = webpackContext;
webpackContext.id = 394;


/***/ }),

/***/ 398:
/***/ (function(module, exports) {

module.exports = "<section class=\"w3-container\">\n    <div class=\"w3-panel w3-red\">\n      <h3>Access Denied</h3>\n      <p>You do not have access to view this page...</p>\n    </div> \n  </section>"

/***/ }),

/***/ 399:
/***/ (function(module, exports) {

module.exports = "<app-menu #menu>\r\n  <h2 class=\"menu-header theme-text-d1\">Admin|Server</h2>\r\n  <div class=\"menu-items w3-cell-row theme-l2\">\r\n    <div *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('structures')}\" (click)=\"menu.activate('structures')\">Structures</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('admin-server', 'players')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('players')}\" (click)=\"menu.activate('players')\">Players</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('admin-server', 'tribes')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('tribes')}\" (click)=\"menu.activate('tribes')\">Tribes</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('admin-server', 'fertilized-eggs')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('fertilized-eggs')}\" (click)=\"menu.activate('fertilized-eggs')\">Fertilized Eggs</div>\r\n  </div>\r\n</app-menu>"

/***/ }),

/***/ 400:
/***/ (function(module, exports) {

module.exports = "<section *ngIf=\"(loaded == false &amp;&amp; !isMenuActive('structures')) || loadedStructures == false &amp;&amp; isMenuActive('structures')\" class=\"w3-container\">\r\n  <div class=\"w3-panel theme-l2\">\r\n    <h3 class=\"theme-text-l1-light\">Loading...</h3>\r\n  </div> \r\n</section>\r\n<section *ngIf=\"(loaded == true &amp;&amp; server == null &amp;&amp; !isMenuActive('structures')) || (loadedStructures == true &amp;&amp; structures == null &amp;&amp; isMenuActive('structures'))\" class=\"w3-container\">\r\n  <div class=\"w3-panel w3-red\">\r\n    <h3>Error!</h3>\r\n    <p>No data could be loaded for the given server key.</p>\r\n  </div> \r\n</section>\r\n<section *ngIf=\"isMenuActive('players') &amp;&amp; server &amp;&amp; dataService.hasFeatureAccess('admin-server', 'players')\" class=\"w3-container\">\r\n  <h2 class=\"theme-text-d1\">Players</h2>\r\n  <div class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Steam Id</th>\r\n          <th>Character Id</th>\r\n          <!--<th>Steam Name</th>-->\r\n          <th>Character Name</th>\r\n          <th>Tribe Name</th>\r\n          <th>Tribe Id</th>\r\n          <th>Structures</th>\r\n          <th>Creatures</th>\r\n          <th>Last Active</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let player of server.Players\">\r\n          <td>{{player.FakeSteamId || player.SteamId}}</td>\r\n          <td>{{player.Id}}</td>\r\n          <!--<td>{{player.SteamName}}</td>-->\r\n          <td><a *ngIf=\"dataService.hasFeatureAccess('pages', 'player', player.SteamId); else players_player_no_link\" [routerLink]=\"'/player/' + player.SteamId\">{{player.CharacterName}}</a><ng-template #players_player_no_link>{{player.CharacterName}}</ng-template></td>\r\n          <td>{{player.TribeName}}</td>\r\n          <td>{{player.TribeId}}</td>\r\n          <td>{{player.StructureCount}}</td>\r\n          <td>{{player.CreatureCount}}</td>\r\n          <td>{{dataService.toRelativeDate(player.LastActiveTime)}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('tribes') &amp;&amp; server &amp;&amp; dataService.hasFeatureAccess('admin-server', 'tribes')\" class=\"w3-container\">\r\n  <h2 class=\"theme-text-d1\">Tribes</h2>\r\n  <div class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Id</th>\r\n          <th>Name</th>\r\n          <th>Members</th>\r\n          <th>Structures</th>\r\n          <th>Creatures</th>\r\n          <th>Last Active</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let tribe of server.Tribes\">\r\n          <td>{{tribe.Id}}</td>\r\n          <td>{{tribe.Name}}</td>\r\n          <td><span *ngFor=\"let member of tribe.MemberSteamIds; let last = last\"><a *ngIf=\"dataService.hasFeatureAccess('pages', 'player', member); else tribes_player_no_link\" [routerLink]=\"'/player/' + member\">{{getTribeMember(member)?.CharacterName || member}}</a><ng-template #tribes_player_no_link>{{getTribeMember(member)?.CharacterName || member}}</ng-template><span *ngIf=\"!last\">, </span></span></td>\r\n          <td>{{tribe.StructureCount}}</td>\r\n          <td>{{tribe.CreatureCount}}</td>\r\n          <td>{{dataService.toRelativeDate(tribe.LastActiveTime)}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('structures') &amp;&amp; structures &amp;&amp; dataService.hasFeatureAccess('admin-server', 'structures')\" class=\"w3-container\">\r\n  <arkmap-structures [serverKey]=\"serverKey\" [mapName]=\"structures?.MapName\" [structures]=\"structures\"></arkmap-structures>\r\n</section>\r\n\r\n<section *ngIf=\"isMenuActive('fertilized-eggs') &amp;&amp; loadedFertilizedEggs == false &amp;&amp; server &amp;&amp; dataService.hasFeatureAccess('admin-server', 'fertilized-eggs')\" class=\"w3-container\">\r\n  <div class=\"w3-panel theme-l2\">\r\n    <h3 class=\"theme-text-l1-light\">Loading...</h3>\r\n  </div> \r\n</section>\r\n\r\n<section *ngIf=\"loadedFertilizedEggs == true &amp;&amp;isMenuActive('fertilized-eggs') &amp;&amp; server &amp;&amp; dataService.hasFeatureAccess('admin-server', 'fertilized-eggs')\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n      <div class=\"w3-cell\"><h2 class=\"theme-text-d1 w3-left\">Egg Summary</h2></div>\r\n      <div class=\"w3-cell w3-cell-middle\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"saveWorld($event)\" class=\"w3-right\" [width]=\"undefined\">Save World</confirm-button></div>\r\n    </div>\r\n\r\n  <div class=\"w3-card-4 w3-margin-bottom w3-responsive\">  \r\n      <header class=\"w3-container theme-d1\">\r\n        <h3>Summary</h3>\r\n      </header>\r\n      <div *ngIf=\"fertilizedEggsCount != 0 || spoiledEggsCount != 0\" class=\"w3-container theme-l1\">\r\n        <p>Total Egg<span *ngIf=\"(totalEggCount) > 1\">s</span>: {{totalEggCount}}</p>\r\n        <p>Fertilized Egg<span *ngIf=\"fertilizedEggsCount > 1\">s</span>: {{fertilizedEggsCount}}</p>\r\n        <p>Spoiled Egg<span *ngIf=\"spoiledEggsCount > 1\">s</span>: {{spoiledEggsCount}}</p>\r\n      </div>\r\n      <div *ngIf=\"fertilizedEggsCount == 0 && spoiledEggsCount == 0\" class=\"w3-container theme-l1\">\r\n        <p>There are no fertilized eggs on the map</p>\r\n      </div>\r\n  </div>\r\n\r\n  <div *ngIf=\"fertilizedEggsCount &amp;&amp; fertilizedEggsCount != 0\" class=\"w3-cell-row\">\r\n      <div class=\"w3-cell\"><h2 class=\"theme-text-d1 w3-left\">Fertilized Eggs</h2></div>\r\n      <div class=\"w3-cell w3-cell-middle\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"destroyAllEggs($event)\" class=\"w3-right\" [width]=\"undefined\">Destroy All Eggs</confirm-button></div>\r\n  </div>\r\n  <div *ngIf=\"fertilizedEggsCount &amp;&amp; fertilizedEggsCount != 0\" class=\"w3-card-4 w3-responsive w3-margin-bottom\">     \r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Creature</th>\r\n          <th>Egg Level</th>\r\n          <th>Spoil Time</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let egg of fertilizedEggsList\">\r\n          <td>{{egg.Dino}}</td>\r\n          <td>{{egg.EggLevel}}</td>\r\n          <td>{{egg.SpoilTime}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table> \r\n  </div>\r\n\r\n  <div *ngIf=\"spoiledEggsCount &amp;&amp; spoiledEggsCount != 0\" class=\"w3-cell-row\">\r\n      <div class=\"w3-cell\"><h2 class=\"theme-text-d1 w3-left\">Spoiled Eggs</h2></div>\r\n      <div class=\"w3-cell w3-cell-middle\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"destroySpoiledEggs($event)\" class=\"w3-right\" [width]=\"undefined\">Destroy Spoiled Eggs</confirm-button></div>\r\n  </div>\r\n  <div *ngIf=\"spoiledEggsCount &amp;&amp; spoiledEggsCount != 0\" class=\"w3-card-4 w3-responsive w3-margin-bottom\"> \r\n   <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Creature</th>\r\n          <th>Egg Level</th>\r\n          <th>Dropped By</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let egg of spoiledEggsList\">\r\n          <td>{{egg.Dino}}</td>\r\n          <td>{{egg.EggLevel}}</td>\r\n          <td *ngIf=\"DroppedBySteamId\"><a href=\"/player/{{egg.DroppedBySteamId}}\">{{egg.DroppedBy}}</a></td>\r\n          <td *ngIf=\"!DroppedBySteamId\">{{egg.DroppedBy}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table> \r\n  </div>\r\n</section>\r\n\r\n<div #contextMenu class=\"contextMenu w3-modal\">\r\n    <div class=\"w3-modal-content w3-card-4 w3-animate-zoom\" (clickOutside)=\"hideContextMenu()\" style=\"font-size: 0;\">\r\n      <ng-container *ngIf=\"modalInfo\">\r\n        <header class=\"w3-container theme-d1\"> \r\n          <span (click)=\"hideContextMenu()\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n          <h2>{{modalInfo.Header}}</h2>\r\n        </header>\r\n        <div class=\"w3-container w3-medium theme-l2\">\r\n          <div class=\"w3-section\">\r\n            {{modalInfo.Message}}\r\n          </div>\r\n        </div>\r\n      </ng-container>\r\n    </div>\r\n  </div>"

/***/ }),

/***/ 401:
/***/ (function(module, exports) {

module.exports = "<simple-notifications [options]=\"notificationOptions\"></simple-notifications>\r\n<router-outlet name=\"menu\"></router-outlet>\r\n<div id=\"page\">\r\n  <div class=\"w3-bar\">\r\n    <breadcrumb prefix=\"Home\" [useBootstrap]=\"false\" class=\"breadcrumb w3-bar-item\"></breadcrumb>\r\n    <div class=\"w3-bar-item w3-right w3-tiny theme-l1\"><span *ngIf=\"dataService.Servers?.User?.SteamId\">Logged in as {{dataService.Servers.User.Name}} | <a [href]=\"getLogoutUrl()\">Logout</a> | </span><span *ngIf=\"!dataService.Servers?.User?.SteamId\"><a href=\"#\" (click)=\"openLogin($event)\">Login</a> | </span>Theme: <a href=\"#\" (click)=\"setTheme('light')\">Light</a> | <a href=\"#\" (click)=\"setTheme('dark')\">Dark</a></div>\r\n  </div>\r\n  <router-outlet></router-outlet>\r\n</div>\r\n<div id=\"modal_login\" class=\"w3-modal\" [style.display]=\"showLogin ? 'block' : 'none'\">\r\n  <div class=\"w3-modal-content w3-card-4 w3-animate-zoom\" (clickOutside)=\"closeLogin($event)\" style=\"font-size: 0;\">\r\n    <header class=\"w3-container theme-d1\"> \r\n      <span (click)=\"showLogin = false\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n      <h2>Log In</h2>\r\n    </header>\r\n    <form class=\"w3-container theme-l2 w3-medium\" method=\"post\" [action]=\"getLoginUrl()\" ngNoForm>\r\n      <div class=\"w3-section\">\r\n        <p>To give you access to personal information, first, we must verify your identity.</p>\r\n        <p>Please authenticate with our app through Steam by clicking on the button below.</p>\r\n        <input name=\"returnUrl\" type=\"hidden\" [value]=\"currentUrl\" />\r\n        <button type=\"submit\" class=\"w3-button w3-block theme-d1 w3-section w3-padding\" name=\"provider\" title=\"Log in using your Steam account\">Go to Steam</button>\r\n      </div>\r\n    </form>\r\n  </div>\r\n</div>"

/***/ }),

/***/ 402:
/***/ (function(module, exports) {

module.exports = "<!--<div class=\"contextMenu\" #contextMenu>\r\n  <ng-container *ngIf=\"currentOwner &amp;&amp; currentArea\">\r\n    <h4 style=\"margin-bottom: 3px;\">{{currentOwner.Name}}</h4>\r\n    <div>Coords: {{currentArea.Latitude | number:'1.0-1'}}, {{currentArea.Longitude | number:'1.0-1'}}</div>\r\n    <div *ngIf=\"currentOwner.LastActiveTime\">Last Active: {{dataService.toRelativeDate(currentOwner.LastActiveTime)}}</div>\r\n    <div class=\"w3-margin-bottom\">{{currentArea.StructureCount | number}} structures</div>\r\n    <button *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\" class=\"w3-button theme-d1\" style=\"width: 100%;\" (click)=\"confirmDestroyCurrentArea()\">Destroy this area</button>\r\n  </ng-container>\r\n</div>-->\r\n<div #contextMenu class=\"contextMenu w3-modal\">\r\n  <div class=\"w3-modal-content w3-card-4 w3-animate-zoom\" (clickOutside)=\"hideContextMenu()\" style=\"font-size: 0;\">\r\n    <ng-container *ngIf=\"currentArea &amp;&amp; currentOwner\">\r\n      <header class=\"w3-container theme-d1\"> \r\n        <span (click)=\"hideContextMenu()\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n        <h2>{{currentOwner.Name}}</h2>\r\n      </header>\r\n      <div class=\"w3-container w3-medium theme-l2\">\r\n        <div class=\"w3-section\">\r\n          Coords: {{currentArea.Latitude | number:'1.0-1'}}, {{currentArea.Longitude | number:'1.0-1'}}<br />\r\n          <ng-container *ngIf=\"currentOwner.LastActiveTime\">Last Active: {{dataService.toRelativeDate(currentOwner.LastActiveTime)}}<br /></ng-container>\r\n          {{currentArea.StructureCount | number}} structures\r\n        </div>\r\n        <div class=\"w3-section\"><button class=\"w3-button theme-d1\" style=\"width: 100%;\" (click)=\"setSelectedOwner(currentOwner)\">Show only areas for this team</button></div>\r\n        <div class=\"w3-section\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"destroyCurrentArea($event)\" [width]=\"100\">Destroy this area</confirm-button></div>\r\n      </div>\r\n    </ng-container>\r\n    <ng-container *ngIf=\"currentOwner &amp;&amp; !currentArea\">\r\n      <header class=\"w3-container theme-d1\"> \r\n        <span (click)=\"hideContextMenu()\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n        <h2>{{currentOwner.Name}}</h2>\r\n      </header>\r\n      <div class=\"w3-container w3-medium theme-l2\">\r\n        <div class=\"w3-section\">\r\n          <ng-container *ngIf=\"currentOwner.LastActiveTime\">Last Active: {{dataService.toRelativeDate(currentOwner.LastActiveTime)}}<br /></ng-container>\r\n          {{currentOwner.AreaCount | number}} areas<br />\r\n          {{currentOwner.StructureCount | number}} structures<br />\r\n          {{currentOwner.CreatureCount | number}} creatures\r\n        </div>\r\n        <div class=\"w3-section\"><button class=\"w3-button theme-d1\" style=\"width: 100%;\" (click)=\"setSelectedOwner(currentOwner)\">Show only areas for this team</button></div>\r\n        <div class=\"w3-section\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"destroyAllStructuresForTeam($event)\" [width]=\"100\">Destroy all structures</confirm-button></div>\r\n        <div class=\"w3-section\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"destroyDinosForTeam($event)\" [width]=\"100\">Destroy all creatures</confirm-button></div>\r\n      </div>\r\n    </ng-container>\r\n    <ng-container *ngIf=\"modalInfo\">\r\n      <header class=\"w3-container theme-d1\"> \r\n        <span (click)=\"hideContextMenu()\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n        <h2>{{modalInfo.Header}}</h2>\r\n      </header>\r\n      <div class=\"w3-container w3-medium theme-l2\">\r\n        <div class=\"w3-section\">\r\n          {{modalInfo.Message}}\r\n        </div>\r\n      </div>\r\n    </ng-container>\r\n  </div>\r\n</div>\r\n\r\n<div class=\"w3-cell-row\">\r\n  <div class=\"w3-cell\"><a id=\"structures\"></a><h2 class=\"theme-text-d1 w3-left\">Structures</h2></div>\r\n  <div class=\"w3-cell w3-cell-middle\" *ngIf=\"dataService.hasFeatureAccess('admin-server', 'structures-rcon')\"><confirm-button (callback)=\"saveWorld($event)\" class=\"w3-right\" [width]=\"undefined\">Save World</confirm-button></div>\r\n</div>\r\n\r\n<div class=\"wrapper\">\r\n  <div class=\"buttons\">\r\n    <button class=\"w3-button theme-d1\" style=\"padding: 3px 6px;\" (click)=\"zoomIn()\"><i class=\"material-icons w3-xxlarge\">add</i></button>\r\n    <button class=\"w3-button theme-d1\" style=\"padding: 3px 6px;\" (click)=\"zoomOut()\"><i class=\"material-icons w3-xxlarge\">remove</i></button>\r\n  </div>\r\n  <div class=\"map\" #map></div>\r\n</div>\r\n\r\n<ng-container *ngIf=\"ownersSorted\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><h2 class=\"theme-text-d1 w3-left\">Player/Tribe Locations</h2></div>\r\n    <div class=\"w3-cell w3-cell-middle\">\r\n      <div class=\"w3-clear\">\r\n        <!--<button class=\"w3-button theme-d1 w3-right w3-margin-left\" (click)=\"reset($event)\">All</button>\r\n        <button class=\"w3-button theme-d1 w3-right\" (click)=\"reset($event)\">None</button>-->\r\n        <button class=\"w3-button theme-d1 w3-right\" (click)=\"reset($event)\">Reset</button>\r\n      </div>\r\n    </div>\r\n  </div>\r\n  <div class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Name</th>\r\n          <th>Type</th>\r\n          <th style=\"cursor: pointer;\" title=\"Sort by Location Count\" (click)=\"setOwnerSort('locations')\">A#</th>\r\n          <th style=\"cursor: pointer;\" title=\"Sort by Structure Count\" (click)=\"setOwnerSort('structures')\">S#</th>\r\n          <th title=\"Creature Count\">C#</th>\r\n          <th style=\"cursor: pointer;\" title=\"Sort by Last Active Time\" (click)=\"setOwnerSort('lastactive')\">Last Active</th>\r\n          <th></th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let owner of ownersSorted\">\r\n          <td><input type=\"radio\" [(ngModel)]=\"selectedOwner\" [value]=\"owner\" (change)=\"updateSelection()\" /> {{owner.Name}}</td>\r\n          <td>{{owner.Type}}</td>\r\n          <td>{{owner.AreaCount}}</td>\r\n          <td>{{owner.StructureCount}}</td>\r\n          <td>{{owner.CreatureCount}}</td>\r\n          <td>{{dataService.toRelativeDate(owner.LastActiveTime)}}</td>\r\n          <td><button class=\"w3-button theme-d1 w3-right\" (click)=\"showOwnerModal($event, owner)\">Options</button></td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</ng-container>"

/***/ }),

/***/ 403:
/***/ (function(module, exports) {

module.exports = "<button #confirmButton class=\"w3-button\" [ngClass]=\"{'theme-d1': !confirming, 'theme-e1': confirming}\" [style.width.%]=\"width\" (click)=\"onClick($event)\">\r\n  <ng-content *ngIf=\"!confirming\"></ng-content>\r\n  <ng-container *ngIf=\"confirming\">Tripple tap to confirm...</ng-container>\r\n</button>"

/***/ }),

/***/ 404:
/***/ (function(module, exports) {

module.exports = "<section class=\"w3-container\">\n    <div class=\"w3-panel w3-red\">\n      <h3>Connection error</h3>\n      <p>The application was unable to connect to the Web API. This could be due to a configuration error...</p>\n    </div> \n  </section>"

/***/ }),

/***/ 405:
/***/ (function(module, exports) {

module.exports = "<div *ngIf=\"(numEnabledModes | async) > 1\" class=\"w3-bar theme-l2 w3-card-4 w3-margin-bottom\">\r\n  <ng-container *ngFor=\"let mode of _modes; trackBy: trackByKey\">\r\n    <button *ngIf=\"mode.enabled | async\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': isCurrentMode(mode.key)}\" [style.width.%]=\"(100/(numEnabledModes | async))\" (click)=\"setCurrentMode(mode.key)\">{{mode.name}}</button>\r\n  </ng-container>\r\n</div>\r\n<div class=\"w3-card-4 w3-responsive\">\r\n  <table class=\"w3-table-all border-theme\">\r\n    <thead>\r\n      <tr class=\"theme-d1\">\r\n        <ng-container *ngFor=\"let column of _columnTemplates; trackBy: trackByKey\">\r\n          <th *ngIf=\"showColumn(column.key)\" (click)=\"orderBy(column.key, $event)\" title=\"{{column.title}}\" [ngClass]=\"{'orderBy': column.orderBy}\">\r\n            <ng-template\r\n              *ngIf=\"column.headerTemplate\"\r\n              [ngTemplateOutlet]=\"column.headerTemplate\">\r\n            </ng-template>\r\n          </th>\r\n        </ng-container>\r\n      </tr>\r\n    </thead>\r\n    <tbody>\r\n      <tr *ngIf=\"(_rows$ | async)?.length == 0\">\r\n        <td [colSpan]=\"currentModeEnabledColumnCount()\">No matching entries...</td>\r\n      </tr>\r\n      <tr *ngFor=\"let row of _rows$ | async | slice:_fromRow:_fromRow+_numRows;  trackBy: trackByRow\">\r\n        <ng-container *ngFor=\"let column of _columnTemplates; trackBy: trackByKey\">\r\n          <td *ngIf=\"showColumn(column.key)\">\r\n            <ng-template\r\n              *ngIf=\"column.cellTemplate\"\r\n              [ngTemplateOutlet]=\"column.cellTemplate\"\r\n              [ngTemplateOutletContext]=\"{$implicit: row}\">\r\n            </ng-template>\r\n          </td>\r\n        </ng-container>\r\n      </tr>\r\n    </tbody>\r\n  </table>\r\n</div>\r\n<div class=\"w3-cell-row w3-margin-top\">\r\n  <div class=\"w3-cell w3-cell-middle\">\r\n    <table class=\"w3-responsive w3-right w3-small\">\r\n      <tr>\r\n        <td>\r\n          <div class=\"w3-bar w3-border border-theme\">\r\n            <a (click)=\"setFirstPage()\" [ngClass]=\"{'disabled': isFirstPage()}\" class=\"w3-button w3-border-right border-theme\">«</a>\r\n            <a (click)=\"setPrevPage()\" [ngClass]=\"{'disabled': isFirstPage()}\" class=\"w3-button device-tiny-show\">❮</a>\r\n            <a (click)=\"setPrevPage()\" [ngClass]=\"{'disabled': isFirstPage()}\" class=\"w3-button w3-border-right border-theme device-tiny-hide\">❮</a>\r\n            <span class=\"device-tiny-hide\">&nbsp;{{_fromRow}} - {{getLastRowOffset()}} of {{_totalRows}}&nbsp;</span>\r\n            <a (click)=\"setLastPage()\" [ngClass]=\"{'disabled': isLastPage()}\" class=\"w3-button w3-border-left border-theme w3-right\">»</a>\r\n            <a (click)=\"setNextPage()\" [ngClass]=\"{'disabled': isLastPage()}\" class=\"w3-button w3-border-left border-theme w3-right\">❯</a>\r\n          </div>\r\n        </td>\r\n        <td>\r\n          <select [ngModel]=\"_numRows\" (ngModelChange)=\"setViewLimit($event)\" class=\"w3-select w3-border border-theme theme-l1\" style=\"max-width: 200px; padding: 12px 5px;\">\r\n            <option *ngFor=\"let opt of _viewOptions\" [value]=\"opt.value\">{{opt.text}}</option>\r\n          </select>\r\n        </td>\r\n      </tr>\r\n    </table>\r\n  </div>\r\n</div>"

/***/ }),

/***/ 406:
/***/ (function(module, exports) {

module.exports = "<section class=\"w3-container\">\n  <button class=\"w3-button\" [ngClass]=\"{'theme-d1': demoMode, 'theme-l2': !demoMode, 'theme-hover': !demoMode}\" (click)=\"toggleDemoMode()\"><i *ngIf=\"demoMode\" class=\"material-icons\" style=\"margin: -5px 5px -5px -5px; vertical-align: middle;\">check</i>{{(demoMode ? \"Demo Mode Enabled\" : \"Enable Demo Mode\")}}</button>\n</section>"

/***/ }),

/***/ 407:
/***/ (function(module, exports) {

module.exports = "<div id=\"menu\" class=\"w3-sidebar theme-l2\" style=\"min-height: 52px;\"> \r\n  <h2 class=\"logo\"></h2>\r\n  <button id=\"menubtn\" class=\"w3-button w3-xlarge w3-display-topright\" (click)=\"toggleMenu()\">☰</button>\r\n  <section id=\"menucontent\" class=\"w3-container\"  [ngClass]=\"{'hide': !menuVisible}\">\r\n    <ng-content></ng-content>\r\n  </section>\r\n</div>"

/***/ }),

/***/ 408:
/***/ (function(module, exports) {

module.exports = "<app-menu #menu>\r\n  <h2 class=\"menu-header theme-text-d1\">Player</h2>\r\n  <div class=\"menu-items w3-cell-row theme-l2\">\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'profile', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('profile')}\" (click)=\"menu.activate('profile')\">Profile</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'creatures', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('creatures')}\" (click)=\"menu.activate('creatures')\">Creatures</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'creatures-cloud', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('creatures_cloud')}\" (click)=\"menu.activate('creatures_cloud')\">Creatures (Cloud)</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'breeding', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('breeding')}\" (click)=\"menu.activate('breeding')\">Breeding</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'crops', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('crop_plots')}\" (click)=\"menu.activate('crop_plots')\">Crops</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'generators', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('electrical_generators')}\" (click)=\"menu.activate('electrical_generators')\">Electrical Generators</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'kibbles-eggs', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('kibbles_and_eggs')}\" (click)=\"menu.activate('kibbles_and_eggs')\">Kibbles and Eggs</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('player', 'tribelog', steamId)\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('tribelog')}\" (click)=\"menu.activate('tribelog')\">Tribe Log</div>\r\n  </div>\r\n</app-menu>"

/***/ }),

/***/ 409:
/***/ (function(module, exports) {

module.exports = "<section *ngIf=\"loaded == false\" class=\"w3-container\">\r\n  <div class=\"w3-panel theme-l2\">\r\n    <h3 class=\"theme-text-l1-light\">Loading...</h3>\r\n  </div> \r\n</section>\r\n<section *ngIf=\"loaded == true &amp;&amp; player == null\" class=\"w3-container\">\r\n  <div class=\"w3-panel w3-red\">\r\n    <h3>Error!</h3>\r\n    <p>No data could be loaded for the given steam id.</p>\r\n  </div> \r\n</section>\r\n<section *ngIf=\"!isMenuActive('creatures_cloud') &amp;&amp; player != undefined\" class=\"w3-container\">\r\n  <h2 class=\"theme-text-d1\">Servers</h2>\r\n  <div *ngIf=\"player?.Servers\" class=\"w3-bar theme-l2 w3-card-4\">\r\n    <button *ngFor=\"let server of keysGetter(player?.Servers)\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': active(server)}\" [style.width.%]=\"serverWidth()\" (click)=\"activate(server)\">{{server}}</button>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('profile') &amp;&amp; player != undefined &amp;&amp; dataService.hasFeatureAccess('player', 'profile', steamId)\" class=\"w3-container\">\r\n  <a id=\"player\"></a><h2 class=\"theme-text-d1\">Player</h2>\r\n  <div class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Character Name</th>\r\n          <th *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">Gender</th>\r\n          <th>Tribe Name</th>\r\n          <th>Steam Id</th>\r\n          <th>Tribe Id</th>\r\n          <th *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">Level</th>\r\n          <th *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">Engram Points</th>\r\n          <th *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">Lat</th>\r\n          <th *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">Lng</th>\r\n          <th>Saved At</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr>\r\n          <td>{{player?.Servers[serverKey]?.CharacterName}}</td>\r\n          <td *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">{{player?.Servers[serverKey]?.Gender}}</td>\r\n          <td>{{player?.Servers[serverKey]?.TribeName}}</td>\r\n          <td>{{player?.Servers[serverKey]?.FakeSteamId || player?.Servers[serverKey]?.SteamId}}</td>\r\n          <td>{{player?.Servers[serverKey]?.TribeId}}</td>\r\n          <td *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">{{player?.Servers[serverKey]?.Level}}</td>\r\n          <td *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">{{player?.Servers[serverKey]?.EngramPoints | number}}</td>\r\n          <td *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">{{player?.Servers[serverKey]?.Latitude | number:'1.1-1'}}</td>\r\n          <td *ngIf=\"dataService.hasFeatureAccess('player', 'profile-detailed', steamId)\">{{player?.Servers[serverKey]?.Longitude | number:'1.1-1'}}</td>\r\n          <td>{{dataService.toDate(player?.Servers[serverKey]?.SavedAt)}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('creatures') &amp;&amp; player != undefined &amp;&amp; dataService.hasFeatureAccess('player', 'creatures', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><a id=\"creatures\"></a><h2 class=\"theme-text-d1 w3-left\">Creatures <span class=\"w3-tag w3-large theme-d1\">{{filteredCreatures.length}}</span></h2></div>\r\n    <div class=\"w3-cell w3-cell-middle\"><button class=\"w3-button theme-d1 w3-right\" (click)=\"openMap($event)\">Show Map</button></div>\r\n  </div>\r\n  <div *ngIf=\"!(player.Servers[serverKey]?.Creatures?.length > 0)\">There are no creatures...</div>\r\n  <ng-container *ngIf=\"player.Servers[serverKey]?.Creatures?.length > 0\">\r\n    <div class=\"inner-addon right-addon\">\r\n      <i *ngIf=\"creaturesFilter != null &amp;&amp; creaturesFilter != ''\" class=\"material-icons\" style=\"cursor: pointer;\" (click)=\"creaturesFilter = ''; filterAndSort();\">close</i>\r\n      <input [ngModel]=\"creaturesFilter\" (ngModelChange)=\"creaturesFilter = $event; filterAndSort();\" class=\"w3-input w3-border w3-round-xlarge w3-large w3-margin-bottom border-theme theme-l1\" placeholder=\"Filter\" />\r\n    </div>\r\n    <div *ngIf=\"numCreatureTabs() > 1\" class=\"w3-bar theme-l2 w3-card-4 w3-margin-bottom\">\r\n      <button href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCreaturesMode('status')}\" [style.width.%]=\"(100/numCreatureTabs())\" (click)=\"activateCreaturesMode('status')\">Overview / Status</button>\r\n      <button *ngIf=\"dataService.hasFeatureAccess('player', 'creatures-basestats', steamId)\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCreaturesMode('stats')}\" [style.width.%]=\"(100/numCreatureTabs())\" (click)=\"activateCreaturesMode('stats')\">Base Stats</button>\r\n      <button *ngIf=\"dataService.hasFeatureAccess('player', 'creatures-ids', steamId)\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCreaturesMode('ids')}\" [style.width.%]=\"(100/numCreatureTabs())\" (click)=\"activateCreaturesMode('ids')\">IDs</button>\r\n    </div>\r\n    <div class=\"w3-card-4 w3-responsive\">\r\n      <table class=\"w3-table-all border-theme\">\r\n        <thead>\r\n          <tr class=\"theme-d1\">\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Name\" (click)=\"setCreaturesSort('name')\">Name</th>\r\n            <!--<th>ClassName</th>-->\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Species\" (click)=\"setCreaturesSort('species')\">Species</th>\r\n            <!--<th>Aliases</th>-->\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Gender\" (click)=\"setCreaturesSort('gender')\">Gender</th>\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Base Level\" (click)=\"setCreaturesSort('base_level')\">Base Level</th>\r\n            <ng-container *ngIf=\"activeCreaturesMode('status')\">\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Level\" (click)=\"setCreaturesSort('level')\">Level</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Imprint\" (click)=\"setCreaturesSort('imprint')\">Imprint</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Food\" (click)=\"setCreaturesSort('food')\">Food</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Latitude\" (click)=\"setCreaturesSort('latitude')\">Lat</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Longitude\" (click)=\"setCreaturesSort('longitude')\">Lng</th>\r\n              <th>Status</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Owner\" (click)=\"setCreaturesSort('owner')\">Owner</th>\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('stats')\">\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Health\" (click)=\"setCreaturesSort('stat_health')\">HP</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Stamina\" (click)=\"setCreaturesSort('stat_stamina')\">ST</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Oxygen\" (click)=\"setCreaturesSort('stat_oxygen')\">OX</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Food\" (click)=\"setCreaturesSort('stat_food')\">FO</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Weight\" (click)=\"setCreaturesSort('stat_weight')\">WE</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Melee\" (click)=\"setCreaturesSort('stat_melee')\">ME</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Speed\" (click)=\"setCreaturesSort('stat_speed')\">SP</th>\r\n              <!--<th></th>-->\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('ids')\">\r\n              <th style=\"cursor: pointer;\" title=\"Sort by ID1\" (click)=\"setCreaturesSort('id1')\">ID1</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by ID2\" (click)=\"setCreaturesSort('id2')\">ID2</th>\r\n            </ng-container>\r\n          </tr>\r\n        </thead>\r\n        <tbody>\r\n          <tr *ngIf=\"!(filteredCreatures?.length > 0)\"><td [colSpan]=\"(activeCreaturesMode('ids') ? 6 : 11)\">No matching creatures...</td></tr>\r\n          <tr *ngFor=\"let creature of filteredCreatures\">\r\n            <td>{{creature.Name}}</td>\r\n            <!--<td>{{creature.ClassName}}</td>-->\r\n            <td>{{creature.Species}}</td>\r\n            <!--<td>{{creature.Aliases}}</td>-->\r\n            <td>{{creature.Gender}}</td>\r\n            <td>{{creature.BaseLevel}}</td>\r\n            <ng-container *ngIf=\"activeCreaturesMode('status')\">\r\n              <td><span *ngIf=\"creature.BaseLevel != creature.Level\">{{creature.Level}}</span></td>\r\n              <td>{{creature.Imprint | percent:'1.0-0'}}</td>\r\n              <td>\r\n                <div *ngIf=\"creature.FoodStatus != null\" class=\"app-green-light w3-round\" style=\"width: 6em; position: relative;\">\r\n                  <div style=\"position: absolute; left: 50%; transform: translate(-50%, 0%); color: white;\">{{creature.FoodStatus | percent:'1.0-0'}}</div>\r\n                  <div class=\"theme-c1 w3-round\" [style.width.%]=\"creature.FoodStatus * 100\">&nbsp;</div>\r\n                </div>\r\n              </td>\r\n              <td>{{creature.Latitude | number:'1.1-1'}}</td>\r\n              <td>{{creature.Longitude | number:'1.1-1'}}</td>\r\n              <td>\r\n                <span *ngIf=\"haveMatingCooldown(creature)\">Next mating {{dataService.toRelativeDate(creature.NextMating)}}</span>\r\n                <div *ngIf=\"creature.BabyAge != null\">\r\n                  <div>\r\n                    <div class=\"w3-cell w3-cell-middle\">Baby</div>\r\n                    <div class=\"w3-cell w3-cell-middle\">\r\n                      <div class=\"app-green-light w3-round\" style=\"width: 4em; position: relative; margin: 0em 0.5em;\">\r\n                        <div style=\"position: absolute; left: 50%; transform: translate(-50%, 0%); color: white;\">{{creature.BabyAge | percent:'1.0-0'}}</div>\r\n                        <div class=\"theme-c1 w3-round\" [style.width.%]=\"creature.BabyAge * 100\">&nbsp;</div>\r\n                      </div>\r\n                    </div>\r\n                    <div class=\"w3-cell\">cuddle {{dataService.toRelativeDate(creature.BabyNextCuddle)}}</div>\r\n                  </div>\r\n                </div>\r\n              </td>\r\n              <td>{{creature.OwnerType}}</td>\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('stats')\">\r\n              <td>{{creature.BaseStats?.Health}}</td>\r\n              <td>{{creature.BaseStats?.Stamina}}</td>\r\n              <td>{{creature.BaseStats?.Oxygen}}</td>\r\n              <td>{{creature.BaseStats?.Food}}</td>\r\n              <td>{{creature.BaseStats?.Weight}}</td>\r\n              <td>{{creature.BaseStats?.Melee}}</td>\r\n              <td>{{creature.BaseStats?.MovementSpeed}}</td>\r\n              <!--<td><i class=\"material-icons w3-medium\" style=\"cursor: pointer;\" (click)=\"copyCreature(creature)\">content_copy</i></td>-->\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('ids')\">\r\n              <td>{{creature.Id1}}</td>\r\n              <td>{{creature.Id2}}</td>\r\n            </ng-container>\r\n          </tr>\r\n        </tbody>\r\n      </table>\r\n    </div>\r\n  </ng-container>\r\n</section>\r\n<section *ngIf=\"isMenuActive('breeding') &amp;&amp; player != undefined &amp;&amp; dataService.hasFeatureAccess('player', 'breeding', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><a id=\"imprint_timers\"></a><h2 class=\"theme-text-d1 w3-left\">Breeding <span class=\"w3-tag w3-large theme-d1\">{{imprintCreatures.length}}</span></h2></div>\r\n      <div class=\"w3-cell w3-cell-middle\"><button class=\"w3-button w3-right\" [ngClass]=\"{'theme-d1': imprintNotifications, 'theme-l2': !imprintNotifications, 'theme-hover': !imprintNotifications}\" (click)=\"imprintNotifications = !imprintNotifications\"><i *ngIf=\"imprintNotifications\" class=\"material-icons\" style=\"margin: -5px 5px -5px -5px; vertical-align: middle;\">check</i>{{(imprintNotifications ? \"Notifications Enabled\" : \"Enable Notifications\")}}</button></div>\r\n  </div>\r\n  <div *ngIf=\"!(imprintCreatures?.length > 0)\">There are no baby creatures...</div>\r\n  <div *ngIf=\"imprintCreatures?.length > 0\" class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Name</th>\r\n          <th>Species</th>\r\n          <th>Gender</th>\r\n          <th>Base Level</th>\r\n          <th>Imprint</th>\r\n          <th>Progress</th>\r\n          <th>Fully Grown At</th>\r\n          <th>Next Imprint</th>\r\n          <th *ngIf=\"imprintNotifications\"></th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let creature of imprintCreatures; let index = index\" [ngClass]=\"{'odd': index % 2 == 1}\">\r\n          <td>{{creature.Name}}</td>\r\n          <td>{{creature.Species}}</td>\r\n          <td>{{creature.Gender}}</td>\r\n          <td>{{creature.BaseLevel}}</td>\r\n          <td>{{creature.Imprint | percent:'1.0-0'}}</td>\r\n          <td>\r\n            <div class=\"app-green-light w3-round\" style=\"width: 6em; position: relative;\">\r\n              <div style=\"position: absolute; left: 50%; transform: translate(-50%, 0%); color: white;\">{{creature.BabyAge | percent:'1.0-0'}}</div>\r\n              <div class=\"theme-c1 w3-round\" [style.width.%]=\"creature.BabyAge * 100\">&nbsp;</div>\r\n            </div>\r\n          </td>\r\n          <td>{{dataService.toDate(creature.BabyFullyGrown)}}</td>\r\n          <td><timer [time]=\"creature.BabyNextCuddle\" [notification]=\"imprintNotifications\" [state]=\"getStateForCreature(creature)\"></timer></td>\r\n          <td *ngIf=\"imprintNotifications\"><input style=\"top: 0;\" class=\"w3-check w3-right\" type=\"checkbox\" [checked]=\"getStateForCreature(creature).imprintNotifications\" (change)=\"toggleImprintNotificationForCreature(creature)\" /></td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n  <!--<p *ngIf=\"getCurrentServer() != undefined\" class=\"w3-small\">\r\n    Last Update {{getCurrentServer().LastUpdate}}, Next Update {{getCurrentServer().NextUpdate || '-'}}\r\n  </p>-->\r\n</section>\r\n<section *ngIf=\"isMenuActive('kibbles_and_eggs') &amp;&amp; player?.Servers[serverKey] &amp;&amp; dataService.hasFeatureAccess('player', 'kibbles-eggs', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><a id=\"kibblesandeggs\"></a><h2 class=\"theme-text-d1 w3-left\">Kibbles and Eggs <span class=\"w3-tag w3-large theme-d1\">{{sumKibbleAndEggs() | number:0.0-0}}</span></h2></div>\r\n  </div>\r\n  <div *ngIf=\"!(player.Servers[serverKey].KibblesAndEggs?.length > 0)\">There are no kibbles or eggs...</div>\r\n  <div *ngIf=\"player.Servers[serverKey].KibblesAndEggs?.length > 0\" class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Name</th>\r\n          <th>Kibbles</th>\r\n          <th>Eggs</th>\r\n          <th>Total</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let ke of player.Servers[serverKey].KibblesAndEggs\">\r\n          <td>{{ke.Name}}</td>\r\n          <td>{{ke.KibbleCount}}</td>\r\n          <td>{{ke.EggCount}}</td>\r\n          <td>{{ke.KibbleCount + ke.EggCount}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('creatures_cloud') &amp;&amp; haveCluster()\" class=\"w3-container\">\r\n  <h2 class=\"theme-text-d1\">Clusters</h2>\r\n  <div *ngIf=\"player?.Clusters\" class=\"w3-bar theme-l2 w3-card-4\">\r\n    <button *ngFor=\"let cluster of keysGetter(player?.Clusters)\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCluster(cluster)}\" [style.width.%]=\"clusterWidth()\" (click)=\"activateCluster(cluster)\">{{cluster}}</button>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('creatures_cloud') &amp;&amp; !haveCluster() &amp;&amp; dataService.hasFeatureAccess('player', 'creatures-cloud', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><h2 class=\"theme-text-d1 w3-left\">Creatures</h2></div>\r\n  </div>\r\n  <div>There are no creatures in the cloud...</div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('creatures_cloud') &amp;&amp; haveCluster() &amp;&amp; dataService.hasFeatureAccess('player', 'creatures-cloud', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><h2 class=\"theme-text-d1 w3-left\">Creatures <span class=\"w3-tag w3-large theme-d1\">{{filteredClusterCreatures.length}}</span></h2></div>\r\n  </div>\r\n  <div *ngIf=\"!(player.Clusters[clusterKey]?.Creatures?.length > 0)\">There are no creatures in the cloud...</div>\r\n  <div *ngIf=\"player.Clusters[clusterKey]?.Creatures?.length > 0\" class=\"inner-addon right-addon\">\r\n    <i *ngIf=\"creaturesClusterFilter != null &amp;&amp; creaturesClusterFilter != ''\" class=\"material-icons\" style=\"cursor: pointer;\" (click)=\"creaturesClusterFilter = ''; filterCluster();\">close</i>\r\n    <input [ngModel]=\"creaturesClusterFilter\" (ngModelChange)=\"creaturesClusterFilter = $event; filterCluster();\" class=\"w3-input w3-border w3-round-xlarge w3-large w3-margin-bottom border-theme theme-l1\" placeholder=\"Filter\" />\r\n  </div>\r\n  <div *ngIf=\"player.Clusters[clusterKey]?.Creatures?.length > 0\" class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Name</th>\r\n          <th>Species</th>\r\n          <th>Level</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngIf=\"!(filteredClusterCreatures?.length > 0)\"><td colspan=\"3\">No matching creatures...</td></tr>\r\n        <tr *ngFor=\"let creature of filteredClusterCreatures\">\r\n          <td>{{creature.Name}}</td>\r\n          <td>{{creature.Species}}</td>\r\n          <td>{{creature.Level}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('crop_plots') &amp;&amp; player?.Servers[serverKey] &amp;&amp; dataService.hasFeatureAccess('player', 'crops', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><a id=\"cropplots\"></a><h2 class=\"theme-text-d1 w3-left\">Crops</h2></div>\r\n  </div>\r\n  <div *ngIf=\"!(player.Servers[serverKey].CropPlots?.length > 0)\">There are no crops...</div>\r\n  <div *ngIf=\"player.Servers[serverKey].CropPlots?.length > 0\" class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Crop</th>\r\n          <th>Size</th>\r\n          <th>Fertilizer %</th>\r\n          <th>Fertilizer Units</th>\r\n          <th>Water</th>\r\n          <th>Lat</th>\r\n          <th>Lng</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let cp of player.Servers[serverKey].CropPlots\">\r\n          <td>{{(cp.PlantedCropName || cp.PlantedCropClassName)}}</td>\r\n          <td>{{cp.Size}}</td>\r\n          <td>\r\n            <div class=\"app-green-light w3-round\" style=\"width: 6em; position: relative;\">\r\n              <div style=\"position: absolute; left: 50%; transform: translate(-50%, 0%); color: white;\">{{(cp.FertilizerQuantity / cp.FertilizerMax) | percent:'1.0-0'}}</div>\r\n              <div class=\"theme-c1 w3-round\" [style.width.%]=\"(cp.FertilizerQuantity / cp.FertilizerMax) * 100\">&nbsp;</div>\r\n            </div>\r\n          </td>\r\n          <td>{{cp.FertilizerQuantity | number}}</td>\r\n          <td>{{cp.WaterAmount | number:'1.0-0'}}</td>\r\n          <td>{{cp.Latitude | number:'1.1-1'}}</td>\r\n          <td>{{cp.Longitude | number:'1.1-1'}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('electrical_generators') &amp;&amp; player?.Servers[serverKey] &amp;&amp; dataService.hasFeatureAccess('player', 'generators', steamId)\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><a id=\"electricalgenerators\"></a><h2 class=\"theme-text-d1 w3-left\">Electrical Generators</h2></div>\r\n  </div>\r\n  <div *ngIf=\"!(player.Servers[serverKey].ElectricalGenerators?.length > 0)\">There are no electrical generators...</div>\r\n  <div *ngIf=\"player.Servers[serverKey].ElectricalGenerators?.length > 0\" class=\"w3-card-4 w3-responsive\">\r\n    <table class=\"w3-table-all border-theme\">\r\n      <thead>\r\n        <tr class=\"theme-d1\">\r\n          <th>Gasoline %</th>\r\n          <th>Gasoline Quantity</th>\r\n          <th>Activated</th>\r\n          <th>Lat</th>\r\n          <th>Lng</th>\r\n        </tr>\r\n      </thead>\r\n      <tbody>\r\n        <tr *ngFor=\"let eg of player.Servers[serverKey].ElectricalGenerators\">\r\n          <td>\r\n            <div *ngIf=\"eg.Activated == true\" class=\"app-green-light w3-round\" style=\"width: 6em; position: relative;\">\r\n              <div style=\"position: absolute; left: 50%; transform: translate(-50%, 0%); color: white;\">{{(eg.GasolineQuantity / 800.0) | percent:'1.0-0'}}</div>\r\n              <div class=\"theme-c1 w3-round\" [style.width.%]=\"(eg.GasolineQuantity / 800.0) * 100\">&nbsp;</div>\r\n            </div>\r\n          </td>\r\n          <td>{{eg.GasolineQuantity | number}}</td>\r\n          <td>{{(eg.Activated == true ? \"Yes\" : \"No\")}}</td>\r\n          <td>{{eg.Latitude | number:'1.1-1'}}</td>\r\n          <td>{{eg.Longitude | number:'1.1-1'}}</td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('tribelog') &amp;&amp; player?.Servers[serverKey] &amp;&amp; dataService.hasFeatureAccess('player', 'tribelog', steamId)\" class=\"w3-container\">\r\n    <div class=\"w3-cell-row\">\r\n      <div class=\"w3-cell\"><a id=\"tribelog\"></a><h2 class=\"theme-text-d1 w3-left\">Tribe Log</h2></div>\r\n    </div>\r\n    <div *ngIf=\"!(player.Servers[serverKey].TribeLog?.length > 0)\">There are no tribe logs...</div>\r\n    <ng-container *ngIf=\"player.Servers[serverKey].TribeLog?.length > 0\">\r\n      <div class=\"inner-addon right-addon\">\r\n          <i *ngIf=\"tribeLogFilter != null &amp;&amp; tribeLogFilter != ''\" class=\"material-icons\" style=\"cursor: pointer;\" (click)=\"tribeLogFilter = '';\">close</i>\r\n          <input [ngModel]=\"tribeLogFilter\" (ngModelChange)=\"tribeLogFilter = $event;\" class=\"w3-input w3-border w3-round-xlarge w3-large w3-margin-bottom border-theme theme-l1\" placeholder=\"Filter\" />\r\n      </div>\r\n      <ark-data-table [rows]=\"player.Servers[serverKey].TribeLog\" [filter]=\"tribeLogFilter\" [filterFunction]=\"tribeLogFilterFunction\">\r\n        <ark-dt-mode name=\"Default\" key=\"default\" columnKeys=\"day,time,message\"></ark-dt-mode>\r\n        <ark-dt-column key=\"day\">\r\n          <ng-template ark-dt-header>\r\n            Day\r\n          </ng-template>\r\n          <ng-template let-log ark-dt-cell>\r\n              {{log.Day}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n        <ark-dt-column key=\"time\">\r\n          <ng-template ark-dt-header>\r\n            Time\r\n          </ng-template>\r\n          <ng-template let-log ark-dt-cell>\r\n              {{log.Time}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n        <ark-dt-column key=\"message\">\r\n          <ng-template ark-dt-header>\r\n            Message\r\n          </ng-template>\r\n          <ng-template let-log ark-dt-cell>\r\n              <span [outerHTML]=\"log.Message | sanitizeHtml\"></span>\r\n          </ng-template>\r\n        </ark-dt-column>\r\n      </ark-data-table>\r\n    </ng-container>\r\n  </section>\r\n<div id=\"modal_map\" class=\"w3-modal\" [style.display]=\"showMap ? 'block' : 'none'\">\r\n  <div class=\"w3-modal-content w3-card-4 w3-animate-zoom\" (clickOutside)=\"closeMap($event)\" style=\"font-size: 0;\">\r\n  <header class=\"w3-container theme-d1\"> \r\n    <span (click)=\"showMap = false\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n    <h2>Map</h2>\r\n  </header>\r\n  <arkmap [mapName]=\"player?.MapNames[serverKey]\" [points]=\"points\"></arkmap>\r\n  </div>\r\n</div>"

/***/ }),

/***/ 410:
/***/ (function(module, exports) {

module.exports = "<app-menu #menu>\r\n  <h2 class=\"menu-header theme-text-d1\">Servers</h2>\r\n  <div class=\"menu-items w3-cell-row theme-l2\">\r\n    <div class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('overview')}\" (click)=\"menu.activate('overview')\">Overview</div>\r\n    <ng-container *ngIf=\"dataService.Servers != undefined &amp;&amp; dataService.hasFeatureAccess('home', 'serverdetails')\">\r\n      <div *ngFor=\"let server of dataService.Servers.Servers\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active(server.Key)}\" (click)=\"menu.activate(server.Key)\">{{server.Key}}</div>\r\n    </ng-container>\r\n  </div>\r\n</app-menu>"

/***/ }),

/***/ 411:
/***/ (function(module, exports) {

module.exports = "<section *ngIf=\"isMenuActive('overview') &amp;&amp; dataService.UserSteamId != undefined &amp;&amp; dataService.hasFeatureAccess('home', 'myprofile') &amp;&amp; dataService.hasFeatureAccess('pages', 'player', dataService.UserSteamId)\" class=\"w3-container\">\r\n  <h3 class=\"theme-text-d1\">My Profile</h3>\r\n  <div class=\"w3-card-4 w3-margin-bottom\">\r\n    <header class=\"w3-container theme-d1\">\r\n      <h3>Hello, {{dataService.Servers.User.Name}}</h3>\r\n    </header>\r\n    <div class=\"w3-container theme-l1\">\r\n      <p>\r\n        Find your tames, view base stats and keep track of their food status. Get notified of pending imprints, the amount of fertilizer and gasoline remaining in your crops and generators. This and much more is available in your profile.\r\n      </p>\r\n      <p><a [routerLink]=\"'/player/' + dataService.UserSteamId\" class=\"\" style=\"text-decoration: none;\">View my profile&nbsp;❯</a></p>\r\n    </div>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('overview') &amp;&amp; dataService.Servers != undefined &amp;&amp; serverCount > 0 &amp;&amp; dataService.hasFeatureAccess('home', 'serverlist')\" class=\"w3-container\">\r\n  <h3 class=\"theme-text-d1\">Servers</h3>\r\n  <div *ngFor=\"let server of dataService.Servers.Servers\" class=\"w3-card-4 w3-margin-bottom\">\r\n    <header class=\"w3-container theme-d1\">\r\n      <h3><a *ngIf=\"dataService.hasFeatureAccess('pages', 'server'); else server_no_link\" [routerLink]=\"'/server/' + server.Key\" style=\"text-decoration: none;\"><ng-container *ngIf=\"server.MapName\">{{server.MapName}} - </ng-container>{{server.Key}}</a><ng-template #server_no_link><ng-container *ngIf=\"server.MapName\">{{server.MapName}} - </ng-container>{{server.Key}}</ng-template></h3>\r\n    </header>\r\n    <div class=\"w3-container theme-l1\">\r\n      <p class=\"w3-small\">\r\n        Last Update {{server.LastUpdate}}, Next Update {{server.NextUpdate || '-'}}\r\n      </p>\r\n      <p *ngIf=\"dataService.hasFeatureAccess('pages', 'server')\"><a [routerLink]=\"'/server/' + server.Key\" class=\"\" style=\"text-decoration: none;\">View server ❯</a></p>\r\n      <p *ngIf=\"dataService.hasFeatureAccess('pages', 'admin-server')\"><a [routerLink]=\"'/admin/' + server.Key\" class=\"\" style=\"text-decoration: none;\">Admin ❯</a></p>\r\n    </div>\r\n  </div>\r\n</section>\r\n<section *ngIf=\"isMenuActive('overview') &amp;&amp; dataService.Servers != undefined &amp;&amp; dataService.hasFeatureAccess('home', 'online')\" class=\"w3-container\">\r\n  <h3 class=\"theme-text-d1\">Online <span class=\"w3-tag w3-large theme-d1\">{{onlinePlayerCount}}</span></h3>\r\n  <div *ngIf=\"onlinePlayerCount == 0; else online_players_list\">There are no players online...</div>\r\n  <ng-template #online_players_list>\r\n    <div class=\"w3-card-4 w3-responsive\">\r\n      <table class=\"w3-table w3-striped w3-bordered border-theme\">\r\n          <tr class=\"theme-d1\">\r\n              <th>Steam Name</th>\r\n              <th>Character Name</th>\r\n              <th>Tribe Name</th>\r\n              <th>Discord Tag</th>\r\n              <th>Server</th>\r\n              <th>Time Online</th>\r\n          </tr>\r\n          <ng-container *ngFor=\"let server of dataService.Servers.Servers\">\r\n            <tr *ngFor=\"let player of server.OnlinePlayers\">\r\n              <td>{{player.SteamName}}</td>\r\n              <td>{{player.CharacterName}}</td>\r\n              <td>{{player.TribeName}}</td>\r\n              <td>{{player.DiscordName}}</td>\r\n              <td>{{server.Key}}</td>\r\n              <td>{{player.TimeOnline}}</td>\r\n            </tr>\r\n          </ng-container>\r\n      </table>\r\n    </div>\r\n  </ng-template>\r\n</section>\r\n<ng-container *ngFor=\"let server of dataService.Servers?.Servers\">\r\n  <section *ngIf=\"isMenuActive(server.Key) &amp;&amp; dataService.Servers != undefined &amp;&amp; dataService.hasFeatureAccess('home', 'serverdetails')\" class=\"w3-container\">\r\n    <div class=\"w3-card-4 w3-responsive w3-margin-bottom\">\r\n      <header class=\"w3-container theme-d1\">\r\n        <h3><a *ngIf=\"dataService.hasFeatureAccess('pages', 'server'); else serverdetails_no_link\" [routerLink]=\"'/server/' + server.Key\" style=\"text-decoration: none;\">{{server.Name}}</a><ng-template #serverdetails_no_link>{{server.Name}}</ng-template></h3>\r\n      </header>\r\n      <div class=\"w3-container theme-l1\">\r\n        <table class=\"w3-table w3-bordered w3-small border-theme serverdetails\">\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Address</th>\r\n              <td style=\"width: max-content;\">{{server.Address}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Version</th>\r\n              <td>{{server.Version}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Player Slots</th>\r\n              <td>{{server.OnlinePlayerMax}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Map</th>\r\n              <td>{{server.MapName}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">In-Game Day</th>\r\n              <td>{{server.InGameTime}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Tamed Creatures</th>\r\n              <td>{{server.TamedCreatureCount | number}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Cloud Creatures</th>\r\n              <td>{{server.CloudCreatureCount | number}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Wild Creatures</th>\r\n              <td>{{server.WildCreatureCount | number}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Structures</th>\r\n              <td>{{server.StructureCount | number}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Players</th>\r\n              <td>{{server.PlayerCount | number}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Tribes</th>\r\n              <td>{{server.TribeCount | number}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Last Update</th>\r\n              <td>{{server.LastUpdate}}</td>\r\n            </tr>\r\n            <tr>\r\n              <th class=\"theme-text-d1\">Next Update</th>\r\n              <td>{{server.NextUpdate}}</td>\r\n            </tr>\r\n            <tr style=\"border-bottom: none;\">\r\n              <th class=\"theme-text-d1\">Uptime</th>\r\n              <td>{{server.ServerStarted ? dataService.toRelativeDate(server.ServerStarted) : '-'}}</td>\r\n            </tr>\r\n          </table>\r\n      </div>\r\n    </div>\r\n\r\n    <!--<h3 class=\"theme-text-d1\"><a *ngIf=\"dataService.hasFeatureAccess('pages', 'server'); else serverdetails_no_link\" [routerLink]=\"'/server/' + server.Key\" style=\"text-decoration: none;\">{{server.Name}}</a><ng-template #serverdetails_no_link>{{server.Name}}</ng-template></h3>\r\n    <div class=\"w3-responsive w3-margin-bottom\">\r\n      <table class=\"w3-table w3-bordered w3-small border-theme serverdetails\">\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Address</th>\r\n          <td style=\"width: max-content;\">{{server.Address}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Version</th>\r\n          <td>{{server.Version}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Player Slots</th>\r\n          <td>{{server.OnlinePlayerMax}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Map</th>\r\n          <td>{{server.MapName}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">In-Game Time</th>\r\n          <td>{{server.InGameTime}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Tamed Creatures</th>\r\n          <td>{{server.TamedCreatureCount | number}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Cloud Creatures</th>\r\n          <td>{{server.CloudCreatureCount | number}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Wild Creatures</th>\r\n          <td>{{server.WildCreatureCount | number}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Structures</th>\r\n          <td>{{server.StructureCount | number}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Players</th>\r\n          <td>{{server.PlayerCount | number}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Tribes</th>\r\n          <td>{{server.TribeCount | number}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Last Update</th>\r\n          <td>{{server.LastUpdate}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Next Update</th>\r\n          <td>{{server.NextUpdate}}</td>\r\n        </tr>\r\n        <tr>\r\n          <th class=\"theme-text-d1\">Uptime</th>\r\n          <td>{{server.ServerStarted ? dataService.toRelativeDate(server.ServerStarted) : '-'}}</td>\r\n        </tr>\r\n      </table>\r\n    </div>-->\r\n    <ng-container *ngIf=\"dataService.hasFeatureAccess('home', 'online')\">\r\n      <h3 class=\"theme-text-d1\">Online <span class=\"w3-tag w3-large theme-d1\">{{server.OnlinePlayerCount}}</span></h3>\r\n      <div *ngIf=\"server.OnlinePlayerCount == 0; else server_online_players_list\">There are no players online...</div>\r\n      <ng-template #server_online_players_list>\r\n        <div class=\"w3-card-4 w3-responsive w3-margin-bottom\">\r\n          <table *ngIf=\"isMenuActive(server.Key)\" class=\"w3-table w3-striped w3-bordered border-theme\">\r\n              <tr class=\"theme-d1\">\r\n                  <th>Steam Name</th>\r\n                  <th>Character Name</th>\r\n                  <th>Tribe Name</th>\r\n                  <th>Discord Tag</th>\r\n                  <th>Time Online</th>\r\n              </tr>\r\n              <tr *ngFor=\"let player of server.OnlinePlayers\">\r\n                <td>{{player.SteamName}}</td>\r\n                <td>{{player.CharacterName}}</td>\r\n                <td>{{player.TribeName}}</td>\r\n                <td>{{player.DiscordName}}</td>\r\n                <td>{{player.TimeOnline}}</td>\r\n              </tr>\r\n          </table>\r\n        </div>\r\n      </ng-template>\r\n    </ng-container>\r\n  </section>\r\n</ng-container>\r\n<section *ngIf=\"isMenuActive('overview') &amp;&amp; dataService.hasFeatureAccess('home', 'externalresources')\" class=\"w3-container margin-top\">\r\n  <h3 class=\"theme-text-d1\">External Resources</h3>\r\n  <div class=\"w3-card-4 w3-margin-bottom\">\r\n    <header class=\"w3-container theme-d1\">\r\n      <h3>Wiki</h3>\r\n    </header>\r\n    <div class=\"w3-container theme-l1\">\r\n      <p><a href=\"http://ark.gamepedia.com/\" style=\"text-decoration: none;\">Official ARK Survival Evolved Wiki&nbsp;❯</a></p>\r\n    </div>\r\n  </div>\r\n  <div class=\"w3-card-4 w3-margin-bottom\">\r\n    <header class=\"w3-container theme-d1\">\r\n      <h3>Taming Calculators</h3>\r\n    </header>\r\n    <div class=\"w3-container theme-l1\">\r\n      <ul class=\"w3-ul\" style=\"margin: 7px 0px;\">\r\n        <li style=\"padding-left: 0px;\"><a href=\"http://www.survive-ark.com/taming-calculator/\" style=\"text-decoration: none;\">Survive ARK: Taming Calculator&nbsp;❯</a></li>\r\n        <li style=\"padding-left: 0px;\"><a href=\"http://www.dododex.com/\" style=\"text-decoration: none;\">Dododex: Taming Calculator&nbsp;❯</a></li>\r\n      </ul>\r\n    </div>\r\n  </div>\r\n  <div class=\"w3-card-4 w3-margin-bottom\">\r\n    <header class=\"w3-container theme-d1\">\r\n      <h3>Creature Library and Breeding Suggestions</h3>\r\n    </header>\r\n    <div class=\"w3-container theme-l1\">\r\n      <p><a href=\"https://github.com/cadon/ARKStatsExtractor\" style=\"text-decoration: none;\">ARK Smart Breeding&nbsp;❯</a></p>\r\n    </div>\r\n  </div>\r\n</section>"

/***/ }),

/***/ 412:
/***/ (function(module, exports) {

module.exports = "<app-menu #menu>\r\n  <h2 class=\"menu-header theme-text-d1\">Server</h2>\r\n  <div class=\"menu-items w3-cell-row theme-l2\">\r\n    <div *ngIf=\"dataService.hasFeatureAccess('server', 'players')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('players')}\" (click)=\"menu.activate('players')\">Players</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('server', 'tribes')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('tribes')}\" (click)=\"menu.activate('tribes')\">Tribes</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('server', 'wildcreatures-statistics')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('wildcreatures-statistics')}\" (click)=\"menu.activate('wildcreatures-statistics')\">Wild Statistics</div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('server', 'wildcreatures')\" class=\"w3-button w3-cell w3-mobile\" [ngClass]=\"{'theme-d1': menu.active('wildcreatures')}\" (click)=\"menu.activate('wildcreatures')\">Wild Creatures</div>\r\n  </div>\r\n</app-menu>"

/***/ }),

/***/ 413:
/***/ (function(module, exports) {

module.exports = "<section *ngIf=\"loaded == false || ((isMenuActive('wildcreatures') || isMenuActive('wildcreatures-statistics')) &amp;&amp; creaturesLoaded == false)\" class=\"w3-container\">\r\n  <div class=\"w3-panel theme-l2\">\r\n    <h3 class=\"theme-text-l1-light\">Loading...</h3>\r\n  </div> \r\n</section>\r\n<section *ngIf=\"loaded == true &amp;&amp; server == null\" class=\"w3-container\">\r\n  <div class=\"w3-panel w3-red\">\r\n    <h3>Error!</h3>\r\n    <p>No data could be loaded for the given server key.</p>\r\n  </div> \r\n</section>\r\n<section *ngIf=\"isMenuActive('players') &amp;&amp; server &amp;&amp; dataService.hasFeatureAccess('server', 'players')\" class=\"w3-container\">\r\n  <h2 class=\"theme-text-d1\">Players</h2>\r\n  <ark-data-table [rows]=\"filteredPlayers\" trackByProp=\"Id\" [sortFunctions]=\"playerSortFunctions\" orderByColumn=\"last_active\">\r\n    <ark-dt-mode name=\"Default\" key=\"default\" columnKeys=\"character_name,tribe_name,last_active\"></ark-dt-mode>\r\n    <ark-dt-column key=\"character_name\" [orderBy]=\"true\" title=\"Sort by Character Name\" thenSort=\"last_active\">\r\n      <ng-template ark-dt-header>\r\n        Character Name\r\n      </ng-template>\r\n      <ng-template let-player ark-dt-cell>\r\n        <a *ngIf=\"dataService.hasFeatureAccessObservable('pages', 'player', player.SteamId) | async; else player_no_link\" [routerLink]=\"'/player/' + player.SteamId\">{{player.CharacterName}}</a><ng-template #player_no_link>{{player.CharacterName}}</ng-template>\r\n      </ng-template>\r\n    </ark-dt-column>\r\n    <ark-dt-column key=\"tribe_name\" [orderBy]=\"true\" title=\"Sort by Tribe Name\" thenSort=\"character_name\">\r\n      <ng-template ark-dt-header>\r\n        Tribe Name\r\n      </ng-template>\r\n      <ng-template let-player ark-dt-cell>\r\n        {{player.TribeName}}\r\n      </ng-template>\r\n    </ark-dt-column>\r\n    <ark-dt-column key=\"last_active\" [orderBy]=\"true\" title=\"Sort by Last Active\" thenSort=\"character_name\">\r\n      <ng-template ark-dt-header>\r\n        Last Active\r\n      </ng-template>\r\n      <ng-template let-player ark-dt-cell>\r\n        <relative-time [time]=\"player.LastActiveTime\"></relative-time>\r\n      </ng-template>\r\n    </ark-dt-column>\r\n  </ark-data-table>\r\n</section>\r\n<section *ngIf=\"isMenuActive('tribes') &amp;&amp; server &amp;&amp; dataService.hasFeatureAccess('server', 'tribes')\" class=\"w3-container\">\r\n  <h2 class=\"theme-text-d1\">Tribes</h2>\r\n  <ark-data-table [rows]=\"filteredTribes\" trackByProp=\"Id\" [sortFunctions]=\"tribeSortFunctions\" orderByColumn=\"last_active\">\r\n    <ark-dt-mode name=\"Default\" key=\"default\" columnKeys=\"tribe_name,members,last_active\"></ark-dt-mode>\r\n    <ark-dt-column key=\"tribe_name\" [orderBy]=\"true\" title=\"Sort by Tribe Name\" thenSort=\"last_active\">\r\n      <ng-template ark-dt-header>\r\n        Tribe Name\r\n      </ng-template>\r\n      <ng-template let-tribe ark-dt-cell>\r\n          {{tribe.Name}}\r\n      </ng-template>\r\n    </ark-dt-column>\r\n    <ark-dt-column key=\"members\">\r\n      <ng-template ark-dt-header>\r\n        Members\r\n      </ng-template>\r\n      <ng-template let-tribe ark-dt-cell>\r\n          <span *ngFor=\"let member of tribe.MemberSteamIds; let last = last\"><a *ngIf=\"dataService.hasFeatureAccess('pages', 'player', member); else tribe_member_no_link\" [routerLink]=\"'/player/' + member\">{{getTribeMember(member)?.CharacterName || member}}</a><ng-template #tribe_member_no_link>{{getTribeMember(member)?.CharacterName || member}}</ng-template><span *ngIf=\"!last\">, </span></span>\r\n      </ng-template>\r\n    </ark-dt-column>\r\n    <ark-dt-column key=\"last_active\" [orderBy]=\"true\" title=\"Sort by Last Active\" thenSort=\"tribe_name\">\r\n      <ng-template ark-dt-header>\r\n        Last Active\r\n      </ng-template>\r\n      <ng-template let-tribe ark-dt-cell>\r\n        <relative-time [time]=\"tribe.LastActiveTime\"></relative-time>\r\n      </ng-template>\r\n    </ark-dt-column>\r\n  </ark-data-table>\r\n</section>\r\n<section *ngIf=\"isMenuActive('wildcreatures-statistics') &amp;&amp; creaturesLoaded &amp;&amp; dataService.hasFeatureAccess('server', 'wildcreatures-statistics')\" class=\"w3-container\">\r\n    <h2 class=\"theme-text-d1\">Wild Statistics <span class=\"w3-tag w3-large theme-d1\">{{(wild.Statistics.Species?.length || 0) | number}}</span></h2>\r\n    <ark-data-table [rows]=\"wild.Statistics.Species\" trackByProp=\"ClassName\" [sortFunctions]=\"wildStatisticsSortFunctions\" orderByColumn=\"species\">\r\n        <ark-dt-mode name=\"Default\" key=\"default\" columnKeys=\"species,class_name,aliases,count,fraction\"></ark-dt-mode>\r\n        <ark-dt-column key=\"species\" [orderBy]=\"true\" title=\"Sort by Species\" thenSort=\"count\">\r\n          <ng-template ark-dt-header>\r\n            Species\r\n          </ng-template>\r\n          <ng-template let-species ark-dt-cell>\r\n            {{species.Name}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n        <ark-dt-column key=\"class_name\" [orderBy]=\"true\" title=\"Sort by Class Name\" thenSort=\"count\">\r\n          <ng-template ark-dt-header>\r\n            Class Name\r\n          </ng-template>\r\n          <ng-template let-species ark-dt-cell>\r\n            {{species.ClassName}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n        <ark-dt-column key=\"aliases\">\r\n          <ng-template ark-dt-header>\r\n            Aliases\r\n          </ng-template>\r\n          <ng-template let-species ark-dt-cell>\r\n            {{species.Aliases.length > 0 ? species.Aliases.join(', ') : ''}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n        <ark-dt-column key=\"count\" [orderBy]=\"true\" title=\"Sort by Count\" thenSort=\"species\">\r\n          <ng-template ark-dt-header>\r\n            Count\r\n          </ng-template>\r\n          <ng-template let-species ark-dt-cell>\r\n            {{species.Count | number}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n        <ark-dt-column key=\"fraction\" [orderBy]=\"true\" title=\"Sort by Fraction\" thenSort=\"species\">\r\n          <ng-template ark-dt-header>\r\n            Fraction\r\n          </ng-template>\r\n          <ng-template let-species ark-dt-cell>\r\n            {{species.Fraction | percent:'1.0-4'}}\r\n          </ng-template>\r\n        </ark-dt-column>\r\n      </ark-data-table>\r\n</section>\r\n<section *ngIf=\"isMenuActive('wildcreatures') &amp;&amp; creaturesLoaded &amp;&amp; dataService.hasFeatureAccess('server', 'wildcreatures')\" class=\"w3-container\">\r\n  <div class=\"w3-cell-row\">\r\n    <div class=\"w3-cell\"><a id=\"creatures\"></a><h2 class=\"theme-text-d1 w3-left\">Wild Creatures <span class=\"w3-tag w3-large theme-d1\">{{(filteredCreatures?.length || 0) | number}}</span>&nbsp;/&nbsp;<span class=\"w3-tag w3-large theme-d1\">{{(wild?.Statistics?.CreatureCount || 0) | number}}</span></h2></div>\r\n    <div *ngIf=\"dataService.hasFeatureAccess('server', 'wildcreatures-coords')\" class=\"w3-cell w3-cell-middle\"><button class=\"w3-button theme-d1 w3-right\" (click)=\"openMap($event)\">Show Map</button></div>\r\n  </div>\r\n  <div *ngIf=\"!(species?.length > 0)\">There are no creatures...</div>\r\n  <ng-container *ngIf=\"species?.length > 0\">\r\n    <select [ngModel]=\"selectedSpecies\" (ngModelChange)=\"selectedSpecies = $event; filterAndSortWild();\" class=\"w3-select w3-border w3-round-xlarge w3-large w3-margin-bottom border-theme theme-l1\" style=\"padding: 8px;\">\r\n        <option *ngFor=\"let s of species\" [value]=\"s\">{{wild.Species[s].Name || s}}</option>\r\n      </select>\r\n    <!--<div class=\"inner-addon right-addon\">\r\n      <i *ngIf=\"creaturesFilter != null &amp;&amp; creaturesFilter != ''\" class=\"material-icons\" style=\"cursor: pointer;\" (click)=\"creaturesFilter = ''; filterAndSortWild();\">close</i>\r\n      <input [ngModel]=\"creaturesFilter\" (ngModelChange)=\"creaturesFilter = $event; filterAndSortWild();\" class=\"w3-input w3-border w3-round-xlarge w3-large w3-margin-bottom border-theme theme-l1\" placeholder=\"Filter\" />\r\n    </div>-->\r\n    <div *ngIf=\"numCreatureTabs() > 1\" class=\"w3-bar theme-l2 w3-card-4 w3-margin-bottom\">\r\n      <button href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCreaturesMode('status')}\" [style.width.%]=\"(100/numCreatureTabs())\" (click)=\"activateCreaturesMode('status')\">Overview</button>\r\n      <button *ngIf=\"dataService.hasFeatureAccess('server', 'wildcreatures-basestats')\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCreaturesMode('stats')}\" [style.width.%]=\"(100/numCreatureTabs())\" (click)=\"activateCreaturesMode('stats')\">Base Stats</button>\r\n      <button *ngIf=\"dataService.hasFeatureAccess('server', 'wildcreatures-ids')\" href=\"#\" class=\"w3-bar-item w3-button w3-mobile\" [ngClass]=\"{'theme-d1': activeCreaturesMode('ids')}\" [style.width.%]=\"(100/numCreatureTabs())\" (click)=\"activateCreaturesMode('ids')\">IDs</button>\r\n    </div>\r\n    <div class=\"w3-card-4 w3-responsive\">\r\n      <table class=\"w3-table-all border-theme\">\r\n        <thead>\r\n          <tr class=\"theme-d1\">\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Gender\" (click)=\"setCreaturesSort('gender')\">Gender</th>\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Base Level\" (click)=\"setCreaturesSort('base_level')\">Base Level</th>\r\n            <th style=\"cursor: pointer;\" title=\"Sort by Tameable\" (click)=\"setCreaturesSort('tameable')\">Tameable</th>\r\n            <ng-container *ngIf=\"activeCreaturesMode('status') && dataService.hasFeatureAccess('server', 'wildcreatures-coords')\">\r\n              <th style=\"cursor: pointer;\" title=\"Sort by X\" (click)=\"setCreaturesSort('x')\">X</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Y\" (click)=\"setCreaturesSort('y')\">Y</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Z\" (click)=\"setCreaturesSort('z')\">Z</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Latitude\" (click)=\"setCreaturesSort('latitude')\">Lat</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Longitude\" (click)=\"setCreaturesSort('longitude')\">Lng</th>\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('stats')\">\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Health\" (click)=\"setCreaturesSort('stat_health')\">HP</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Stamina\" (click)=\"setCreaturesSort('stat_stamina')\">ST</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Oxygen\" (click)=\"setCreaturesSort('stat_oxygen')\">OX</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Food\" (click)=\"setCreaturesSort('stat_food')\">FO</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Weight\" (click)=\"setCreaturesSort('stat_weight')\">WE</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Melee\" (click)=\"setCreaturesSort('stat_melee')\">ME</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by Speed\" (click)=\"setCreaturesSort('stat_speed')\">SP</th>\r\n              <!--<th></th>-->\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('ids')\">\r\n              <th style=\"cursor: pointer;\" title=\"Sort by ID1\" (click)=\"setCreaturesSort('id1')\">ID1</th>\r\n              <th style=\"cursor: pointer;\" title=\"Sort by ID2\" (click)=\"setCreaturesSort('id2')\">ID2</th>\r\n            </ng-container>\r\n          </tr>\r\n        </thead>\r\n        <tbody>\r\n          <tr *ngIf=\"!(filteredCreatures?.length > 0)\"><td [colSpan]=\"activeCreaturesMode('status') ? 8 : (activeCreaturesMode('stats') ? 10 : 5)\">No matching creatures...</td></tr>\r\n          <tr *ngFor=\"let creature of filteredCreatures\">\r\n            <td>{{creature.Gender}}</td>\r\n            <td>{{creature.BaseLevel}}</td>\r\n            <td>{{(wild.Species[selectedSpecies].IsTameable &amp;&amp; creature.IsTameable == true ? \"Yes\" : \"No\")}}</td>\r\n            <ng-container *ngIf=\"activeCreaturesMode('status') && dataService.hasFeatureAccess('server', 'wildcreatures-coords')\">\r\n              <td>{{creature.X}}</td>\r\n              <td>{{creature.Y}}</td>\r\n              <td>{{creature.Z}}</td>\r\n              <td>{{creature.Latitude | number:'1.1-1'}}</td>\r\n              <td>{{creature.Longitude | number:'1.1-1'}}</td>\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('stats')\">\r\n              <td>{{creature.BaseStats?.Health}}</td>\r\n              <td>{{creature.BaseStats?.Stamina}}</td>\r\n              <td>{{creature.BaseStats?.Oxygen}}</td>\r\n              <td>{{creature.BaseStats?.Food}}</td>\r\n              <td>{{creature.BaseStats?.Weight}}</td>\r\n              <td>{{creature.BaseStats?.Melee}}</td>\r\n              <td>{{creature.BaseStats?.MovementSpeed}}</td>\r\n              <!--<td><i class=\"material-icons w3-medium\" style=\"cursor: pointer;\" (click)=\"copyCreature(creature)\">content_copy</i></td>-->\r\n            </ng-container>\r\n            <ng-container *ngIf=\"activeCreaturesMode('ids')\">\r\n              <td>{{creature.Id1}}</td>\r\n              <td>{{creature.Id2}}</td>\r\n            </ng-container>\r\n          </tr>\r\n        </tbody>\r\n      </table>\r\n    </div>\r\n  </ng-container>\r\n</section>\r\n<div id=\"modal_map\" class=\"w3-modal\" [style.display]=\"showMap ? 'block' : 'none'\">\r\n    <div class=\"w3-modal-content w3-card-4 w3-animate-zoom\" (clickOutside)=\"closeMap($event)\" style=\"font-size: 0;\">\r\n    <header class=\"w3-container theme-d1\"> \r\n      <span (click)=\"showMap = false\" class=\"w3-button theme-d1 w3-xlarge w3-display-topright\">&times;</span>\r\n      <h2>Map</h2>\r\n    </header>\r\n    <arkmap [mapName]=\"server?.MapName\" [points]=\"points\"></arkmap>\r\n    </div>\r\n  </div>"

/***/ }),

/***/ 672:
/***/ (function(module, exports, __webpack_require__) {

module.exports = __webpack_require__(284);


/***/ })

},[672]);
//# sourceMappingURL=main.bundle.js.map