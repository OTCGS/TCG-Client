/// <reference no-default-lib="true"/>
declare var NaN: number;
declare var Infinity: number;
declare function eval(x: string): any;
declare function parseInt(s: string, radix?: number): number;
declare function parseFloat(string: string): number;
declare function isNaN(number: number): bool;
declare function isFinite(number: number): bool;
declare function decodeURI(encodedURI: string): string;
declare function decodeURIComponent(encodedURIComponent: string): string;
declare function encodeURI(uri: string): string;
declare function encodeURIComponent(uriComponent: string): string;
interface PropertyDescriptor {
    configurable?: bool;
    enumerable?: bool;
    value?: any;
    writable?: bool;
    get? (): any;
    set? (v: any): void;
}
interface PropertyDescriptorMap {
    [s: string]: PropertyDescriptor;
}
interface Object {
    toString(): string;
    toLocaleString(): string;
    valueOf(): Object;
    hasOwnProperty(v: string): bool;
    isPrototypeOf(v: Object): bool;
    propertyIsEnumerable(v: string): bool;
    [s: string]: any;
}
declare var Object: {
    new (value?: any): Object;
    (): any;
    (value: any): any;
    prototype: Object;
    getPrototypeOf(o: any): any;
    getOwnPropertyDescriptor(o: any, p: string): PropertyDescriptor;
    getOwnPropertyNames(o: any): string[];
    create(o: any, properties?: PropertyDescriptorMap): any;
    defineProperty(o: any, p: string, attributes: PropertyDescriptor): any;
    defineProperties(o: any, properties: PropertyDescriptorMap): any;
    seal(o: any): any;
    freeze(o: any): any;
    preventExtensions(o: any): any;
    isSealed(o: any): bool;
    isFrozen(o: any): bool;
    isExtensible(o: any): bool;
    keys(o: any): string[];
}
interface Function {
    apply(thisArg: any, ...argArray: any[]): any;
    call(thisArg: any, ...argArray: any[]): any;
    bind(thisArg: any, ...argArray: any[]): Function;
    prototype: any;
    length: number;
}
declare var Function: {
    new (...args: string[]): Function;
    (...args: string[]): Function;
    prototype: Function;
}
interface IArguments {
    [index: number]: any;
    length: number;
    callee: Function;
}
interface String {
    toString(): string;
    charAt(pos: number): string;
    charCodeAt(index: number): number;
    concat(...strings: string[]): string;
    indexOf(searchString: string, position?: number): number;
    lastIndexOf(searchString: string, position?: number): number;
    localeCompare(that: string): number;
    match(regexp: string): string[];
    match(regexp: RegExp): string[];
    replace(searchValue: string, replaceValue: string): string;
    replace(searchValue: string, replaceValue: (substring: string, ...args: any[]) => string): string;
    replace(searchValue: RegExp, replaceValue: string): string;
    replace(searchValue: RegExp, replaceValue: (substring: string, ...args: any[]) => string): string;
    search(regexp: string): number;
    search(regexp: RegExp): number;
    slice(start: number, end?: number): string;
    split(seperator: string, limit?: number): string[];
    split(seperator: RegExp, limit?: number): string[];
    substring(start: number, end?: number): string;
    toLowerCase(): string;
    toLocaleLowerCase(): string;
    toUpperCase(): string;
    toLocaleUpperCase(): string;
    trim(): string;
    length: number;
    substr(from: number, length?: number): string;
}
declare var String: {
    new (value?: any): String;
    (value?: any): string;
    prototype: String;
    fromCharCode(...codes: number[]): string;
}
interface Boolean {
}
declare var Boolean: {
    new (value?: any): Boolean;
    (value?: any): bool;
    prototype: Boolean;
}
interface Number {
    toString(radix?: number): string;
    toFixed(fractionDigits?: number): string;
    toExponential(fractionDigits?: number): string;
    toPrecision(precision: number): string;
}
declare var Number: {
    new (value?: any): Number;
    (value?: any): number;
    prototype: Number;
    MAX_VALUE: number;
    MIN_VALUE: number;
    NaN: number;
    NEGATIVE_INFINITY: number;
    POSITIVE_INFINITY: number;
}
interface Math {
    E: number;
    LN10: number;
    LN2: number;
    LOG2E: number;
    LOG10E: number;
    PI: number;
    SQRT1_2: number;
    SQRT2: number;
    abs(x: number): number;
    acos(x: number): number;
    asin(x: number): number;
    atan(x: number): number;
    atan2(y: number, x: number): number;
    ceil(x: number): number;
    cos(x: number): number;
    exp(x: number): number;
    floor(x: number): number;
    log(x: number): number;
    max(...values: number[]): number;
    min(...values: number[]): number;
    pow(x: number, y: number): number;
    random(): number;
    round(x: number): number;
    sin(x: number): number;
    sqrt(x: number): number;
    tan(x: number): number;
}
declare var Math: Math;
interface Date {
    toString(): string;
    toDateString(): string;
    toTimeString(): string;
    toLocaleString(): string;
    toLocaleDateString(): string;
    toLocaleTimeString(): string;
    valueOf(): number;
    getTime(): number;
    getFullYear(): number;
    getUTCFullYear(): number;
    getMonth(): number;
    getUTCMonth(): number;
    getDate(): number;
    getUTCDate(): number;
    getDay(): number;
    getUTCDay(): number;
    getHours(): number;
    getUTCHours(): number;
    getMinutes(): number;
    getUTCMinutes(): number;
    getSeconds(): number;
    getUTCSeconds(): number;
    getMilliseconds(): number;
    getUTCMilliseconds(): number;
    getTimezoneOffset(): number;
    setTime(time: number): void;
    setMilliseconds(ms: number): void;
    setUTCMilliseconds(ms: number): void;
    setSeconds(sec: number, ms?: number): void;
    setUTCSeconds(sec: number, ms?: number): void;
    setMinutes(min: number, sec?: number, ms?: number): void;
    setUTCMinutes(min: number, sec?: number, ms?: number): void;
    setHours(hours: number, min?: number, sec?: number, ms?: number): void;
    setUTCHours(hours: number, min?: number, sec?: number, ms?: number): void;
    setDate(date: number): void;
    setUTCDate(date: number): void;
    setMonth(month: number, date?: number): void;
    setUTCMonth(month: number, date?: number): void;
    setFullYear(year: number, month?: number, date?: number): void;
    setUTCFullYear(year: number, month?: number, date?: number): void;
    toUTCString(): string;
    toISOString(): string;
    toJSON(key?: any): string;
}
declare var Date: {
    new (): Date;
    new (value: number): Date;
    new (value: string): Date;
    new (year: number, month: number, date?: number, hours?: number, minutes?: number, seconds?: number, ms?: number): Date;
    (): string;
    prototype: Date;
    parse(s: string): number;
    UTC(year: number, month: number, date?: number, hours?: number, minutes?: number, seconds?: number, ms?: number): number;
    now(): number;
}
interface RegExpExecArray {
    [index: number]: string;
    length: number;
    index: number;
    input: string;
    toString(): string;
    toLocaleString(): string;
    concat(...items: string[][]): string[];
    join(seperator?: string): string;
    pop(): string;
    push(...items: string[]): void;
    reverse(): string[];
    shift(): string;
    slice(start: number, end?: number): string[];
    sort(compareFn?: (a: string, b: string) => number): string[];
    splice(start: number): string[];
    splice(start: number, deleteCount: number, ...items: string[]): string[];
    unshift(...items: string[]): number;
    indexOf(searchElement: string, fromIndex?: number): number;
    lastIndexOf(searchElement: string, fromIndex?: number): number;
    every(callbackfn: (value: string, index: number, array: string[]) => bool, thisArg?: any): bool;
    some(callbackfn: (value: string, index: number, array: string[]) => bool, thisArg?: any): bool;
    forEach(callbackfn: (value: string, index: number, array: string[]) => void, thisArg?: any): void;
    map(callbackfn: (value: string, index: number, array: string[]) => any, thisArg?: any): any[];
    filter(callbackfn: (value: string, index: number, array: string[]) => bool, thisArg?: any): string[];
    reduce(callbackfn: (previousValue: any, currentValue: any, currentIndex: number, array: string[]) => any, initialValue?: any): any;
    reduceRight(callbackfn: (previousValue: any, currentValue: any, currentIndex: number, array: string[]) => any, initialValue?: any): any;
}
interface RegExp {
    exec(string: string): RegExpExecArray;
    test(string: string): bool;
    source: string;
    global: bool;
    ignoreCase: bool;
    multiline: bool;
    lastIndex: bool;
}
declare var RegExp: {
    new (pattern: string, flags?: string): RegExp;
    (pattern: string, flags?: string): RegExp;
}
interface Error {
    name: string;
    message: string;
}
declare var Error: {
    new (message?: string): Error;
    (message?: string): Error;
    prototype: Error;
}
interface JSON {
    parse(text: string, reviver?: (key: any, value: any) => any): any;
    stringify(value: any): string;
    stringify(value: any, replacer: (key: string, value: any) => any): string;
    stringify(value: any, replacer: any[]): string;
    stringify(value: any, replacer: (key: string, value: any) => any, space: any): string;
    stringify(value: any, replacer: any[], space: any): string;
}
declare var JSON: JSON;
interface Array {
    toString(): string;
    toLocaleString(): string;
    concat(...items: _element[][]): _element[];
    join(seperator?: string): string;
    pop(): _element;
    push(...items: _element[]): void;
    reverse(): _element[];
    shift(): _element;
    slice(start: number, end?: number): _element[];
    sort(compareFn?: (a: _element, b: _element) => number): _element[];
    splice(start: number): _element[];
    splice(start: number, deleteCount: number, ...items: _element[]): _element[];
    unshift(...items: _element[]): number;
    indexOf(searchElement: _element, fromIndex?: number): number;
    lastIndexOf(searchElement: _element, fromIndex?: number): number;
    every(callbackfn: (value: _element, index: number, array: _element[]) => bool, thisArg?: any): bool;
    some(callbackfn: (value: _element, index: number, array: _element[]) => bool, thisArg?: any): bool;
    forEach(callbackfn: (value: _element, index: number, array: _element[]) => void, thisArg?: any): void;
    map(callbackfn: (value: _element, index: number, array: _element[]) => any, thisArg?: any): any[];
    filter(callbackfn: (value: _element, index: number, array: _element[]) => bool, thisArg?: any): _element[];
    reduce(callbackfn: (previousValue: any, currentValue: any, currentIndex: number, array: _element[]) => any, initialValue?: any): any;
    reduceRight(callbackfn: (previousValue: any, currentValue: any, currentIndex: number, array: _element[]) => any, initialValue?: any): any;
    length: number;
}
declare var Array: {
    new (...items: any[]): any[];
    (...items: any[]): any[];
    isArray(arg: any): bool;
    prototype: Array;
}
interface Window {
    ondragend: (ev: any) => any;
    onkeydown: (ev: any) => any;
    ondragover: (ev: any) => any;
    onkeyup: (ev: any) => any;
    onreset: (ev: any) => any;
    onmouseup: (ev: any) => any;
    ondragstart: (ev: any) => any;
    ondrag: (ev: any) => any;
    onmouseover: (ev: any) => any;
    ondragleave: (ev: any) => any;
    history: any;
    name: string;
    onafterprint: (ev: any) => any;
    onpause: (ev: any) => any;
    onbeforeprint: (ev: any) => any;
    top: Window;
    onmousedown: (ev: any) => any;
    onseeked: (ev: any) => any;
    opener: Window;
    onclick: (ev: any) => any;
    onwaiting: (ev: any) => any;
    ononline: (ev: any) => any;
    ondurationchange: (ev: any) => any;
    frames: Window;
    onblur: (ev: any) => any;
    onemptied: (ev: any) => any;
    onseeking: (ev: any) => any;
    oncanplay: (ev: any) => any;
    onstalled: (ev: any) => any;
    onmousemove: (ev: any) => any;
    onoffline: (ev: any) => any;
    length: number;
    onbeforeunload: (ev: any) => any;
    onratechange: (ev: any) => any;
    onstorage: (ev: any) => any;
    onloadstart: (ev: any) => any;
    ondragenter: (ev: any) => any;
    onsubmit: (ev: any) => any;
    self: Window;
    onprogress: (ev: any) => any;
    ondblclick: (ev: any) => any;
    oncontextmenu: (ev: any) => any;
    onchange: (ev: any) => any;
    onloadedmetadata: (ev: any) => any;
    onplay: (ev: any) => any;
    onerror: any;
    onplaying: (ev: any) => any;
    parent: Window;
    location: any;
    oncanplaythrough: (ev: any) => any;
    onabort: (ev: any) => any;
    onreadystatechange: (ev: any) => any;
    onkeypress: (ev: any) => any;
    frameElement: any;
    onloadeddata: (ev: any) => any;
    onsuspend: (ev: any) => any;
    window: Window;
    onfocus: (ev: any) => any;
    onmessage: (ev: any) => any;
    ontimeupdate: (ev: any) => any;
    onresize: (ev: any) => any;
    navigator: any;
    onselect: (ev: any) => any;
    ondrop: (ev: any) => any;
    onmouseout: (ev: any) => any;
    onended: (ev: any) => any;
    onhashchange: (ev: any) => any;
    onunload: (ev: any) => any;
    onscroll: (ev: any) => any;
    onmousewheel: (ev: any) => any;
    onload: (ev: any) => any;
    onvolumechange: (ev: any) => any;
    oninput: (ev: any) => any;
    alert(message?: string): void;
    focus(): void;
    print(): void;
    prompt(message?: string, defaul?: string): string;
    toString(): string;
    open(url?: string, target?: string, features?: string, replace?: bool): Window;
    close(): void;
    confirm(message?: string): bool;
    postMessage(message: any, targetOrigin: string, ports?: any): void;
    showModalDialog(url?: string, argument?: any, options?: any): any;
    blur(): void;
    getSelection(): any;
}
declare var Window: {
    prototype: Window;
    new (): Window;
}
interface Document {
    doctype: any;
    xmlVersion: string;
    implementation: any;
    xmlEncoding: string;
    xmlStandalone: bool;
    documentElement: any;
    inputEncoding: string;
    body: any;
    createElement(tagName: string): any;
    adoptNode(source: any): any;
    createComment(data: string): any;
    createDocumentFragment(): any;
    getElementsByTagName(tagname: string): any;
    getElementsByTagNameNS(namespaceURI: string, localName: string): any;
    createProcessingInstruction(target: string, data: string): any;
    createElementNS(namespaceURI: string, qualifiedName: string): any;
    createAttribute(name: string): any;
    createTextNode(data: string): any;
    importNode(importedNode: any, deep: bool): any;
    createCDATASection(data: string): any;
    createAttributeNS(namespaceURI: string, qualifiedName: string): any;
    getElementById(elementId: string): any;
}
declare var Document: {
    prototype: Document;
    new (): Document;
}
declare var ondragend: (ev: any) => any;
declare var onkeydown: (ev: any) => any;
declare var ondragover: (ev: any) => any;
declare var onkeyup: (ev: any) => any;
declare var onreset: (ev: any) => any;
declare var onmouseup: (ev: any) => any;
declare var ondragstart: (ev: any) => any;
declare var ondrag: (ev: any) => any;
declare var onmouseover: (ev: any) => any;
declare var ondragleave: (ev: any) => any;
declare var history: any;
declare var name: string;
declare var onafterprint: (ev: any) => any;
declare var onpause: (ev: any) => any;
declare var onbeforeprint: (ev: any) => any;
declare var top: Window;
declare var onmousedown: (ev: any) => any;
declare var onseeked: (ev: any) => any;
declare var opener: Window;
declare var onclick: (ev: any) => any;
declare var onwaiting: (ev: any) => any;
declare var ononline: (ev: any) => any;
declare var ondurationchange: (ev: any) => any;
declare var frames: Window;
declare var onblur: (ev: any) => any;
declare var onemptied: (ev: any) => any;
declare var onseeking: (ev: any) => any;
declare var oncanplay: (ev: any) => any;
declare var onstalled: (ev: any) => any;
declare var onmousemove: (ev: any) => any;
declare var onoffline: (ev: any) => any;
declare var length: number;
declare var onbeforeunload: (ev: any) => any;
declare var onratechange: (ev: any) => any;
declare var onstorage: (ev: any) => any;
declare var onloadstart: (ev: any) => any;
declare var ondragenter: (ev: any) => any;
declare var onsubmit: (ev: any) => any;
declare var self: Window;
declare var onprogress: (ev: any) => any;
declare var ondblclick: (ev: any) => any;
declare var oncontextmenu: (ev: any) => any;
declare var onchange: (ev: any) => any;
declare var onloadedmetadata: (ev: any) => any;
declare var onplay: (ev: any) => any;
declare var onerror: any;
declare var onplaying: (ev: any) => any;
declare var parent: Window;
declare var location: any;
declare var oncanplaythrough: (ev: any) => any;
declare var onabort: (ev: any) => any;
declare var onreadystatechange: (ev: any) => any;
declare var onkeypress: (ev: any) => any;
declare var frameElement: any;
declare var onloadeddata: (ev: any) => any;
declare var onsuspend: (ev: any) => any;
declare var window: Window;
declare var onfocus: (ev: any) => any;
declare var onmessage: (ev: any) => any;
declare var ontimeupdate: (ev: any) => any;
declare var onresize: (ev: any) => any;
declare var navigator: any;
declare var onselect: (ev: any) => any;
declare var ondrop: (ev: any) => any;
declare var onmouseout: (ev: any) => any;
declare var onended: (ev: any) => any;
declare var onhashchange: (ev: any) => any;
declare var onunload: (ev: any) => any;
declare var onscroll: (ev: any) => any;
declare var onmousewheel: (ev: any) => any;
declare var onload: (ev: any) => any;
declare var onvolumechange: (ev: any) => any;
declare var oninput: (ev: any) => any;
declare function alert(message?: string): void;
declare function focus(): void;
declare function print(): void;
declare function prompt(message?: string, defaul?: string): string;
declare function toString(): string;
declare function open(url?: string, target?: string, features?: string, replace?: bool): Window;
declare function close(): void;
declare function confirm(message?: string): bool;
declare function postMessage(message: any, targetOrigin: string, ports?: any): void;
declare function showModalDialog(url?: string, argument?: any, options?: any): any;
declare function blur(): void;
declare function getSelection(): any;
declare function getComputedStyle(elt: any, pseudoElt?: string): any;
declare function attachEvent(any: string, listener: any): bool;
declare function detachEvent(any: string, listener: any): void;
declare var status: string;
declare var onmouseleave: (ev: any) => any;
declare var screenLeft: number;
declare var offscreenBuffering: any;
declare var maxConnectionsPerServer: number;
declare var onmouseenter: (ev: any) => any;
declare var clipboardData: any;
declare var defaultStatus: string;
declare var clientInformation: any;
declare var closed: bool;
declare var onhelp: (ev: any) => any;
declare var external: any;
declare var any: any;
declare var onfocusout: (ev: any) => any;
declare var screenTop: number;
declare var onfocusin: (ev: any) => any;
declare function showModelessDialog(url?: string, argument?: any, options?: any): Window;
declare function navigate(url: string): void;
declare function resizeBy(x?: number, y?: number): void;
declare function item(index: any): any;
declare function resizeTo(x?: number, y?: number): void;
declare function createPopup(arguments?: any): any;
declare function toStaticHTML(html: string): string;
declare function execScript(code: string, language?: string): any;
declare function msWriteProfilerMark(profilerMarkName: string): void;
declare function moveTo(x?: number, y?: number): void;
declare function moveBy(x?: number, y?: number): void;
declare function showHelp(url: string, helpArg?: any, features?: string): void;
declare var performance: any;
declare var outerWidth: number;
declare var pageXOffset: number;
declare var innerWidth: number;
declare var pageYOffset: number;
declare var screenY: number;
declare var outerHeight: number;
declare var screen: any;
declare var innerHeight: number;
declare var screenX: number;
declare function scroll(x?: number, y?: number): void;
declare function scrollBy(x?: number, y?: number): void;
declare function scrollTo(x?: number, y?: number): void;
declare var styleMedia: any;
declare var document: Document;
declare function removeEventListener(type: string, listener: any, useCapture?: bool): void;
declare function addEventListener(type: string, listener: any, useCapture?: bool): void;
declare function dispatchEvent(evt: any): bool;
declare var localStorage: any;
declare var sessionStorage: any;
declare function clearTimeout(handle: number): void;
declare function setTimeout(expression: any, msec?: number, language?: any): number;
declare function clearInterval(handle: number): void;
declare function setInterval(expression: any, msec?: number, language?: any): number;
declare var onpopstate: (ev: any) => any;
declare var applicationCache: any;
declare function matchMedia(mediaQuery: string): any;
declare var animationStartTime: number;
declare function cancelAnimationFrame(handle: number): void;
declare function requestAnimationFrame(callback: any): number;
declare function btoa(rawString: string): string;
declare function atob(encodedString: string): string;
declare var indexedDB: any;
declare var console: any;
declare function importScripts(...urls: string[]): void;
declare type ClassDecorator = <TFunction extends Function>(target: TFunction) => TFunction | void;
declare type PropertyDecorator = (target: Object, propertyKey: string | symbol) => void;
declare type MethodDecorator = <T>(target: Object, propertyKey: string | symbol, descriptor: TypedPropertyDescriptor<T>) => TypedPropertyDescriptor<T> | void;
declare type ParameterDecorator = (target: Object, propertyKey: string | symbol, parameterIndex: number) => void;
interface TypedPropertyDescriptor<T> {
    enumerable?: boolean;
    configurable?: boolean;
    writable?: boolean;
    value?: T;
    get?: () => T;
    set?: (value: T) => void;
}
