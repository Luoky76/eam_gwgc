(function (factory) {
    typeof define === 'function' && define.amd ? define(factory) :
        factory();
}((function () {
    'use strict';

    var commonjsGlobal = typeof globalThis !== 'undefined' ? globalThis : typeof window !== 'undefined' ? window : typeof global !== 'undefined' ? global : typeof self !== 'undefined' ? self : {};

    function createCommonjsModule(fn, module) {
        return module = { exports: {} }, fn(module, module.exports), module.exports;
    }

    var check = function (it) {
        return it && it.Math == Math && it;
    };

    // https://github.com/zloirock/core-js/issues/86#issuecomment-115759028
    var global_1 =
        /* global globalThis -- safe */
        check(typeof globalThis == 'object' && globalThis) ||
        check(typeof window == 'object' && window) ||
        check(typeof self == 'object' && self) ||
        check(typeof commonjsGlobal == 'object' && commonjsGlobal) ||
        // eslint-disable-next-line no-new-func -- fallback
        (function () { return this; })() || Function('return this')();

    var fails = function (exec) {
        try {
            return !!exec();
        } catch (error) {
            return true;
        }
    };

    // Detect IE8's incomplete defineProperty implementation
    var descriptors = !fails(function () {
        return Object.defineProperty({}, 1, { get: function () { return 7; } })[1] != 7;
    });

    var nativePropertyIsEnumerable = {}.propertyIsEnumerable;
    var getOwnPropertyDescriptor$1 = Object.getOwnPropertyDescriptor;

    // Nashorn ~ JDK8 bug
    var NASHORN_BUG = getOwnPropertyDescriptor$1 && !nativePropertyIsEnumerable.call({ 1: 2 }, 1);

    // `Object.prototype.propertyIsEnumerable` method implementation
    // https://tc39.es/ecma262/#sec-object.prototype.propertyisenumerable
    var f$5 = NASHORN_BUG ? function propertyIsEnumerable(V) {
        var descriptor = getOwnPropertyDescriptor$1(this, V);
        return !!descriptor && descriptor.enumerable;
    } : nativePropertyIsEnumerable;

    var objectPropertyIsEnumerable = {
        f: f$5
    };

    var createPropertyDescriptor = function (bitmap, value) {
        return {
            enumerable: !(bitmap & 1),
            configurable: !(bitmap & 2),
            writable: !(bitmap & 4),
            value: value
        };
    };

    var toString = {}.toString;

    var classofRaw = function (it) {
        return toString.call(it).slice(8, -1);
    };

    var split = ''.split;

    // fallback for non-array-like ES3 and non-enumerable old V8 strings
    var indexedObject = fails(function () {
        // throws an error in rhino, see https://github.com/mozilla/rhino/issues/346
        // eslint-disable-next-line no-prototype-builtins -- safe
        return !Object('z').propertyIsEnumerable(0);
    }) ? function (it) {
        return classofRaw(it) == 'String' ? split.call(it, '') : Object(it);
    } : Object;

    // `RequireObjectCoercible` abstract operation
    // https://tc39.es/ecma262/#sec-requireobjectcoercible
    var requireObjectCoercible = function (it) {
        if (it == undefined) throw TypeError("Can't call method on " + it);
        return it;
    };

    // toObject with fallback for non-array-like ES3 strings



    var toIndexedObject = function (it) {
        return indexedObject(requireObjectCoercible(it));
    };

    var isObject = function (it) {
        return typeof it === 'object' ? it !== null : typeof it === 'function';
    };

    // `ToPrimitive` abstract operation
    // https://tc39.es/ecma262/#sec-toprimitive
    // instead of the ES6 spec version, we didn't implement @@toPrimitive case
    // and the second argument - flag - preferred type is a string
    var toPrimitive = function (input, PREFERRED_STRING) {
        if (!isObject(input)) return input;
        var fn, val;
        if (PREFERRED_STRING && typeof (fn = input.toString) == 'function' && !isObject(val = fn.call(input))) return val;
        if (typeof (fn = input.valueOf) == 'function' && !isObject(val = fn.call(input))) return val;
        if (!PREFERRED_STRING && typeof (fn = input.toString) == 'function' && !isObject(val = fn.call(input))) return val;
        throw TypeError("Can't convert object to primitive value");
    };

    var hasOwnProperty = {}.hasOwnProperty;

    var has$1 = function (it, key) {
        return hasOwnProperty.call(it, key);
    };

    var document$1 = global_1.document;
    // typeof document.createElement is 'object' in old IE
    var EXISTS = isObject(document$1) && isObject(document$1.createElement);

    var documentCreateElement = function (it) {
        return EXISTS ? document$1.createElement(it) : {};
    };

    // Thank's IE8 for his funny defineProperty
    var ie8DomDefine = !descriptors && !fails(function () {
        return Object.defineProperty(documentCreateElement('div'), 'a', {
            get: function () { return 7; }
        }).a != 7;
    });

    var nativeGetOwnPropertyDescriptor = Object.getOwnPropertyDescriptor;

    // `Object.getOwnPropertyDescriptor` method
    // https://tc39.es/ecma262/#sec-object.getownpropertydescriptor
    var f$4 = descriptors ? nativeGetOwnPropertyDescriptor : function getOwnPropertyDescriptor(O, P) {
        O = toIndexedObject(O);
        P = toPrimitive(P, true);
        if (ie8DomDefine) try {
            return nativeGetOwnPropertyDescriptor(O, P);
        } catch (error) { /* empty */ }
        if (has$1(O, P)) return createPropertyDescriptor(!objectPropertyIsEnumerable.f.call(O, P), O[P]);
    };

    var objectGetOwnPropertyDescriptor = {
        f: f$4
    };

    var anObject = function (it) {
        if (!isObject(it)) {
            throw TypeError(String(it) + ' is not an object');
        } return it;
    };

    var nativeDefineProperty = Object.defineProperty;

    // `Object.defineProperty` method
    // https://tc39.es/ecma262/#sec-object.defineproperty
    var f$3 = descriptors ? nativeDefineProperty : function defineProperty(O, P, Attributes) {
        anObject(O);
        P = toPrimitive(P, true);
        anObject(Attributes);
        if (ie8DomDefine) try {
            return nativeDefineProperty(O, P, Attributes);
        } catch (error) { /* empty */ }
        if ('get' in Attributes || 'set' in Attributes) throw TypeError('Accessors not supported');
        if ('value' in Attributes) O[P] = Attributes.value;
        return O;
    };

    var objectDefineProperty = {
        f: f$3
    };

    var createNonEnumerableProperty = descriptors ? function (object, key, value) {
        return objectDefineProperty.f(object, key, createPropertyDescriptor(1, value));
    } : function (object, key, value) {
        object[key] = value;
        return object;
    };

    var setGlobal = function (key, value) {
        try {
            createNonEnumerableProperty(global_1, key, value);
        } catch (error) {
            global_1[key] = value;
        } return value;
    };

    var SHARED = '__core-js_shared__';
    var store$1 = global_1[SHARED] || setGlobal(SHARED, {});

    var sharedStore = store$1;

    var functionToString = Function.toString;

    // this helper broken in `3.4.1-3.4.4`, so we can't use `shared` helper
    if (typeof sharedStore.inspectSource != 'function') {
        sharedStore.inspectSource = function (it) {
            return functionToString.call(it);
        };
    }

    var inspectSource = sharedStore.inspectSource;

    var WeakMap$1 = global_1.WeakMap;

    var nativeWeakMap = typeof WeakMap$1 === 'function' && /native code/.test(inspectSource(WeakMap$1));

    var isPure = false;

    var shared = createCommonjsModule(function (module) {
        (module.exports = function (key, value) {
            return sharedStore[key] || (sharedStore[key] = value !== undefined ? value : {});
        })('versions', []).push({
            version: '3.9.1',
            mode: 'global',
            copyright: '© 2021 Denis Pushkarev (zloirock.ru)'
        });
    });

    var id = 0;
    var postfix = Math.random();

    var uid = function (key) {
        return 'Symbol(' + String(key === undefined ? '' : key) + ')_' + (++id + postfix).toString(36);
    };

    var keys = shared('keys');

    var sharedKey = function (key) {
        return keys[key] || (keys[key] = uid(key));
    };

    var hiddenKeys$1 = {};

    var WeakMap = global_1.WeakMap;
    var set, get, has;

    var enforce = function (it) {
        return has(it) ? get(it) : set(it, {});
    };

    var getterFor = function (TYPE) {
        return function (it) {
            var state;
            if (!isObject(it) || (state = get(it)).type !== TYPE) {
                throw TypeError('Incompatible receiver, ' + TYPE + ' required');
            } return state;
        };
    };

    if (nativeWeakMap) {
        var store = sharedStore.state || (sharedStore.state = new WeakMap());
        var wmget = store.get;
        var wmhas = store.has;
        var wmset = store.set;
        set = function (it, metadata) {
            metadata.facade = it;
            wmset.call(store, it, metadata);
            return metadata;
        };
        get = function (it) {
            return wmget.call(store, it) || {};
        };
        has = function (it) {
            return wmhas.call(store, it);
        };
    } else {
        var STATE = sharedKey('state');
        hiddenKeys$1[STATE] = true;
        set = function (it, metadata) {
            metadata.facade = it;
            createNonEnumerableProperty(it, STATE, metadata);
            return metadata;
        };
        get = function (it) {
            return has$1(it, STATE) ? it[STATE] : {};
        };
        has = function (it) {
            return has$1(it, STATE);
        };
    }

    var internalState = {
        set: set,
        get: get,
        has: has,
        enforce: enforce,
        getterFor: getterFor
    };

    var redefine = createCommonjsModule(function (module) {
        var getInternalState = internalState.get;
        var enforceInternalState = internalState.enforce;
        var TEMPLATE = String(String).split('String');

        (module.exports = function (O, key, value, options) {
            var unsafe = options ? !!options.unsafe : false;
            var simple = options ? !!options.enumerable : false;
            var noTargetGet = options ? !!options.noTargetGet : false;
            var state;
            if (typeof value == 'function') {
                if (typeof key == 'string' && !has$1(value, 'name')) {
                    createNonEnumerableProperty(value, 'name', key);
                }
                state = enforceInternalState(value);
                if (!state.source) {
                    state.source = TEMPLATE.join(typeof key == 'string' ? key : '');
                }
            }
            if (O === global_1) {
                if (simple) O[key] = value;
                else setGlobal(key, value);
                return;
            } else if (!unsafe) {
                delete O[key];
            } else if (!noTargetGet && O[key]) {
                simple = true;
            }
            if (simple) O[key] = value;
            else createNonEnumerableProperty(O, key, value);
            // add fake Function#toString for correct work wrapped methods / constructors with methods like LoDash isNative
        })(Function.prototype, 'toString', function toString() {
            return typeof this == 'function' && getInternalState(this).source || inspectSource(this);
        });
    });

    var path = global_1;

    var aFunction$1 = function (variable) {
        return typeof variable == 'function' ? variable : undefined;
    };

    var getBuiltIn = function (namespace, method) {
        return arguments.length < 2 ? aFunction$1(path[namespace]) || aFunction$1(global_1[namespace])
            : path[namespace] && path[namespace][method] || global_1[namespace] && global_1[namespace][method];
    };

    var ceil = Math.ceil;
    var floor = Math.floor;

    // `ToInteger` abstract operation
    // https://tc39.es/ecma262/#sec-tointeger
    var toInteger = function (argument) {
        return isNaN(argument = +argument) ? 0 : (argument > 0 ? floor : ceil)(argument);
    };

    var min$1 = Math.min;

    // `ToLength` abstract operation
    // https://tc39.es/ecma262/#sec-tolength
    var toLength = function (argument) {
        return argument > 0 ? min$1(toInteger(argument), 0x1FFFFFFFFFFFFF) : 0; // 2 ** 53 - 1 == 9007199254740991
    };

    var max = Math.max;
    var min = Math.min;

    // Helper for a popular repeating case of the spec:
    // Let integer be ? ToInteger(index).
    // If integer < 0, let result be max((length + integer), 0); else let result be min(integer, length).
    var toAbsoluteIndex = function (index, length) {
        var integer = toInteger(index);
        return integer < 0 ? max(integer + length, 0) : min(integer, length);
    };

    // `Array.prototype.{ indexOf, includes }` methods implementation
    var createMethod$2 = function (IS_INCLUDES) {
        return function ($this, el, fromIndex) {
            var O = toIndexedObject($this);
            var length = toLength(O.length);
            var index = toAbsoluteIndex(fromIndex, length);
            var value;
            // Array#includes uses SameValueZero equality algorithm
            // eslint-disable-next-line no-self-compare -- NaN check
            if (IS_INCLUDES && el != el) while (length > index) {
                value = O[index++];
                // eslint-disable-next-line no-self-compare -- NaN check
                if (value != value) return true;
                // Array#indexOf ignores holes, Array#includes - not
            } else for (; length > index; index++) {
                if ((IS_INCLUDES || index in O) && O[index] === el) return IS_INCLUDES || index || 0;
            } return !IS_INCLUDES && -1;
        };
    };

    var arrayIncludes = {
        // `Array.prototype.includes` method
        // https://tc39.es/ecma262/#sec-array.prototype.includes
        includes: createMethod$2(true),
        // `Array.prototype.indexOf` method
        // https://tc39.es/ecma262/#sec-array.prototype.indexof
        indexOf: createMethod$2(false)
    };

    var indexOf = arrayIncludes.indexOf;


    var objectKeysInternal = function (object, names) {
        var O = toIndexedObject(object);
        var i = 0;
        var result = [];
        var key;
        for (key in O) !has$1(hiddenKeys$1, key) && has$1(O, key) && result.push(key);
        // Don't enum bug & hidden keys
        while (names.length > i) if (has$1(O, key = names[i++])) {
            ~indexOf(result, key) || result.push(key);
        }
        return result;
    };

    // IE8- don't enum bug keys
    var enumBugKeys = [
        'constructor',
        'hasOwnProperty',
        'isPrototypeOf',
        'propertyIsEnumerable',
        'toLocaleString',
        'toString',
        'valueOf'
    ];

    var hiddenKeys = enumBugKeys.concat('length', 'prototype');

    // `Object.getOwnPropertyNames` method
    // https://tc39.es/ecma262/#sec-object.getownpropertynames
    var f$2 = Object.getOwnPropertyNames || function getOwnPropertyNames(O) {
        return objectKeysInternal(O, hiddenKeys);
    };

    var objectGetOwnPropertyNames = {
        f: f$2
    };

    var f$1 = Object.getOwnPropertySymbols;

    var objectGetOwnPropertySymbols = {
        f: f$1
    };

    // all object keys, includes non-enumerable and symbols
    var ownKeys = getBuiltIn('Reflect', 'ownKeys') || function ownKeys(it) {
        var keys = objectGetOwnPropertyNames.f(anObject(it));
        var getOwnPropertySymbols = objectGetOwnPropertySymbols.f;
        return getOwnPropertySymbols ? keys.concat(getOwnPropertySymbols(it)) : keys;
    };

    var copyConstructorProperties = function (target, source) {
        var keys = ownKeys(source);
        var defineProperty = objectDefineProperty.f;
        var getOwnPropertyDescriptor = objectGetOwnPropertyDescriptor.f;
        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            if (!has$1(target, key)) defineProperty(target, key, getOwnPropertyDescriptor(source, key));
        }
    };

    var replacement = /#|\.prototype\./;

    var isForced = function (feature, detection) {
        var value = data[normalize(feature)];
        return value == POLYFILL ? true
            : value == NATIVE ? false
                : typeof detection == 'function' ? fails(detection)
                    : !!detection;
    };

    var normalize = isForced.normalize = function (string) {
        return String(string).replace(replacement, '.').toLowerCase();
    };

    var data = isForced.data = {};
    var NATIVE = isForced.NATIVE = 'N';
    var POLYFILL = isForced.POLYFILL = 'P';

    var isForced_1 = isForced;

    var getOwnPropertyDescriptor = objectGetOwnPropertyDescriptor.f;






    /*
      options.target      - name of the target object
      options.global      - target is the global object
      options.stat        - export as static methods of target
      options.proto       - export as prototype methods of target
      options.real        - real prototype method for the `pure` version
      options.forced      - export even if the native feature is available
      options.bind        - bind methods to the target, required for the `pure` version
      options.wrap        - wrap constructors to preventing global pollution, required for the `pure` version
      options.unsafe      - use the simple assignment of property instead of delete + defineProperty
      options.sham        - add a flag to not completely full polyfills
      options.enumerable  - export as enumerable property
      options.noTargetGet - prevent calling a getter on target
    */
    var _export = function (options, source) {
        var TARGET = options.target;
        var GLOBAL = options.global;
        var STATIC = options.stat;
        var FORCED, target, key, targetProperty, sourceProperty, descriptor;
        if (GLOBAL) {
            target = global_1;
        } else if (STATIC) {
            target = global_1[TARGET] || setGlobal(TARGET, {});
        } else {
            target = (global_1[TARGET] || {}).prototype;
        }
        if (target) for (key in source) {
            sourceProperty = source[key];
            if (options.noTargetGet) {
                descriptor = getOwnPropertyDescriptor(target, key);
                targetProperty = descriptor && descriptor.value;
            } else targetProperty = target[key];
            FORCED = isForced_1(GLOBAL ? key : TARGET + (STATIC ? '.' : '#') + key, options.forced);
            // contained in target
            if (!FORCED && targetProperty !== undefined) {
                if (typeof sourceProperty === typeof targetProperty) continue;
                copyConstructorProperties(sourceProperty, targetProperty);
            }
            // add a flag to not completely full polyfills
            if (options.sham || (targetProperty && targetProperty.sham)) {
                createNonEnumerableProperty(sourceProperty, 'sham', true);
            }
            // extend global
            redefine(target, key, sourceProperty, options);
        }
    };

    var engineUserAgent = getBuiltIn('navigator', 'userAgent') || '';

    var slice = [].slice;
    var MSIE = /MSIE .\./.test(engineUserAgent); // <- dirty ie9- check

    var wrap = function (scheduler) {
        return function (handler, timeout /* , ...arguments */) {
            var boundArgs = arguments.length > 2;
            var args = boundArgs ? slice.call(arguments, 2) : undefined;
            return scheduler(boundArgs ? function () {
                // eslint-disable-next-line no-new-func -- spec requirement
                (typeof handler == 'function' ? handler : Function(handler)).apply(this, args);
            } : handler, timeout);
        };
    };

    // ie9- setTimeout & setInterval additional parameters fix
    // https://html.spec.whatwg.org/multipage/timers-and-user-prompts.html#timers
    _export({ global: true, bind: true, forced: MSIE }, {
        // `setTimeout` method
        // https://html.spec.whatwg.org/multipage/timers-and-user-prompts.html#dom-settimeout
        setTimeout: wrap(global_1.setTimeout),
        // `setInterval` method
        // https://html.spec.whatwg.org/multipage/timers-and-user-prompts.html#dom-setinterval
        setInterval: wrap(global_1.setInterval)
    });

    var freezing = !fails(function () {
        return Object.isExtensible(Object.preventExtensions({}));
    });

    var internalMetadata = createCommonjsModule(function (module) {
        var defineProperty = objectDefineProperty.f;



        var METADATA = uid('meta');
        var id = 0;

        var isExtensible = Object.isExtensible || function () {
            return true;
        };

        var setMetadata = function (it) {
            defineProperty(it, METADATA, {
                value: {
                    objectID: 'O' + ++id, // object ID
                    weakData: {}          // weak collections IDs
                }
            });
        };

        var fastKey = function (it, create) {
            // return a primitive with prefix
            if (!isObject(it)) return typeof it == 'symbol' ? it : (typeof it == 'string' ? 'S' : 'P') + it;
            if (!has$1(it, METADATA)) {
                // can't set metadata to uncaught frozen object
                if (!isExtensible(it)) return 'F';
                // not necessary to add metadata
                if (!create) return 'E';
                // add missing metadata
                setMetadata(it);
                // return object ID
            } return it[METADATA].objectID;
        };

        var getWeakData = function (it, create) {
            if (!has$1(it, METADATA)) {
                // can't set metadata to uncaught frozen object
                if (!isExtensible(it)) return true;
                // not necessary to add metadata
                if (!create) return false;
                // add missing metadata
                setMetadata(it);
                // return the store of weak collections IDs
            } return it[METADATA].weakData;
        };

        // add metadata on freeze-family methods calling
        var onFreeze = function (it) {
            if (freezing && meta.REQUIRED && isExtensible(it) && !has$1(it, METADATA)) setMetadata(it);
            return it;
        };

        var meta = module.exports = {
            REQUIRED: false,
            fastKey: fastKey,
            getWeakData: getWeakData,
            onFreeze: onFreeze
        };

        hiddenKeys$1[METADATA] = true;
    });
    internalMetadata.REQUIRED;
    internalMetadata.fastKey;
    internalMetadata.getWeakData;
    internalMetadata.onFreeze;

    var engineIsNode = classofRaw(global_1.process) == 'process';

    var process$1 = global_1.process;
    var versions = process$1 && process$1.versions;
    var v8 = versions && versions.v8;
    var match, version;

    if (v8) {
        match = v8.split('.');
        version = match[0] + match[1];
    } else if (engineUserAgent) {
        match = engineUserAgent.match(/Edge\/(\d+)/);
        if (!match || match[1] >= 74) {
            match = engineUserAgent.match(/Chrome\/(\d+)/);
            if (match) version = match[1];
        }
    }

    var engineV8Version = version && +version;

    var nativeSymbol = !!Object.getOwnPropertySymbols && !fails(function () {
        /* global Symbol -- required for testing */
        return !Symbol.sham &&
            // Chrome 38 Symbol has incorrect toString conversion
            // Chrome 38-40 symbols are not inherited from DOM collections prototypes to instances
            (engineIsNode ? engineV8Version === 38 : engineV8Version > 37 && engineV8Version < 41);
    });

    var useSymbolAsUid = nativeSymbol
        /* global Symbol -- safe */
        && !Symbol.sham
        && typeof Symbol.iterator == 'symbol';

    var WellKnownSymbolsStore = shared('wks');
    var Symbol$1 = global_1.Symbol;
    var createWellKnownSymbol = useSymbolAsUid ? Symbol$1 : Symbol$1 && Symbol$1.withoutSetter || uid;

    var wellKnownSymbol = function (name) {
        if (!has$1(WellKnownSymbolsStore, name) || !(nativeSymbol || typeof WellKnownSymbolsStore[name] == 'string')) {
            if (nativeSymbol && has$1(Symbol$1, name)) {
                WellKnownSymbolsStore[name] = Symbol$1[name];
            } else {
                WellKnownSymbolsStore[name] = createWellKnownSymbol('Symbol.' + name);
            }
        } return WellKnownSymbolsStore[name];
    };

    var iterators = {};

    var ITERATOR$5 = wellKnownSymbol('iterator');
    var ArrayPrototype$1 = Array.prototype;

    // check on default Array iterator
    var isArrayIteratorMethod = function (it) {
        return it !== undefined && (iterators.Array === it || ArrayPrototype$1[ITERATOR$5] === it);
    };

    var aFunction = function (it) {
        if (typeof it != 'function') {
            throw TypeError(String(it) + ' is not a function');
        } return it;
    };

    // optional / simple context binding
    var functionBindContext = function (fn, that, length) {
        aFunction(fn);
        if (that === undefined) return fn;
        switch (length) {
            case 0: return function () {
                return fn.call(that);
            };
            case 1: return function (a) {
                return fn.call(that, a);
            };
            case 2: return function (a, b) {
                return fn.call(that, a, b);
            };
            case 3: return function (a, b, c) {
                return fn.call(that, a, b, c);
            };
        }
        return function (/* ...args */) {
            return fn.apply(that, arguments);
        };
    };

    var TO_STRING_TAG$3 = wellKnownSymbol('toStringTag');
    var test = {};

    test[TO_STRING_TAG$3] = 'z';

    var toStringTagSupport = String(test) === '[object z]';

    var TO_STRING_TAG$2 = wellKnownSymbol('toStringTag');
    // ES3 wrong here
    var CORRECT_ARGUMENTS = classofRaw(function () { return arguments; }()) == 'Arguments';

    // fallback for IE11 Script Access Denied error
    var tryGet = function (it, key) {
        try {
            return it[key];
        } catch (error) { /* empty */ }
    };

    // getting tag from ES6+ `Object.prototype.toString`
    var classof = toStringTagSupport ? classofRaw : function (it) {
        var O, tag, result;
        return it === undefined ? 'Undefined' : it === null ? 'Null'
            // @@toStringTag case
            : typeof (tag = tryGet(O = Object(it), TO_STRING_TAG$2)) == 'string' ? tag
                // builtinTag case
                : CORRECT_ARGUMENTS ? classofRaw(O)
                    // ES3 arguments fallback
                    : (result = classofRaw(O)) == 'Object' && typeof O.callee == 'function' ? 'Arguments' : result;
    };

    var ITERATOR$4 = wellKnownSymbol('iterator');

    var getIteratorMethod = function (it) {
        if (it != undefined) return it[ITERATOR$4]
            || it['@@iterator']
            || iterators[classof(it)];
    };

    var iteratorClose = function (iterator) {
        var returnMethod = iterator['return'];
        if (returnMethod !== undefined) {
            return anObject(returnMethod.call(iterator)).value;
        }
    };

    var Result = function (stopped, result) {
        this.stopped = stopped;
        this.result = result;
    };

    var iterate = function (iterable, unboundFunction, options) {
        var that = options && options.that;
        var AS_ENTRIES = !!(options && options.AS_ENTRIES);
        var IS_ITERATOR = !!(options && options.IS_ITERATOR);
        var INTERRUPTED = !!(options && options.INTERRUPTED);
        var fn = functionBindContext(unboundFunction, that, 1 + AS_ENTRIES + INTERRUPTED);
        var iterator, iterFn, index, length, result, next, step;

        var stop = function (condition) {
            if (iterator) iteratorClose(iterator);
            return new Result(true, condition);
        };

        var callFn = function (value) {
            if (AS_ENTRIES) {
                anObject(value);
                return INTERRUPTED ? fn(value[0], value[1], stop) : fn(value[0], value[1]);
            } return INTERRUPTED ? fn(value, stop) : fn(value);
        };

        if (IS_ITERATOR) {
            iterator = iterable;
        } else {
            iterFn = getIteratorMethod(iterable);
            if (typeof iterFn != 'function') throw TypeError('Target is not iterable');
            // optimisation for array iterators
            if (isArrayIteratorMethod(iterFn)) {
                for (index = 0, length = toLength(iterable.length); length > index; index++) {
                    result = callFn(iterable[index]);
                    if (result && result instanceof Result) return result;
                } return new Result(false);
            }
            iterator = iterFn.call(iterable);
        }

        next = iterator.next;
        while (!(step = next.call(iterator)).done) {
            try {
                result = callFn(step.value);
            } catch (error) {
                iteratorClose(iterator);
                throw error;
            }
            if (typeof result == 'object' && result && result instanceof Result) return result;
        } return new Result(false);
    };

    var anInstance = function (it, Constructor, name) {
        if (!(it instanceof Constructor)) {
            throw TypeError('Incorrect ' + (name ? name + ' ' : '') + 'invocation');
        } return it;
    };

    var ITERATOR$3 = wellKnownSymbol('iterator');
    var SAFE_CLOSING = false;

    try {
        var called = 0;
        var iteratorWithReturn = {
            next: function () {
                return { done: !!called++ };
            },
            'return': function () {
                SAFE_CLOSING = true;
            }
        };
        iteratorWithReturn[ITERATOR$3] = function () {
            return this;
        };
        // eslint-disable-next-line no-throw-literal -- required for testing
        Array.from(iteratorWithReturn, function () { throw 2; });
    } catch (error) { /* empty */ }

    var checkCorrectnessOfIteration = function (exec, SKIP_CLOSING) {
        if (!SKIP_CLOSING && !SAFE_CLOSING) return false;
        var ITERATION_SUPPORT = false;
        try {
            var object = {};
            object[ITERATOR$3] = function () {
                return {
                    next: function () {
                        return { done: ITERATION_SUPPORT = true };
                    }
                };
            };
            exec(object);
        } catch (error) { /* empty */ }
        return ITERATION_SUPPORT;
    };

    var defineProperty$1 = objectDefineProperty.f;



    var TO_STRING_TAG$1 = wellKnownSymbol('toStringTag');

    var setToStringTag = function (it, TAG, STATIC) {
        if (it && !has$1(it = STATIC ? it : it.prototype, TO_STRING_TAG$1)) {
            defineProperty$1(it, TO_STRING_TAG$1, { configurable: true, value: TAG });
        }
    };

    var aPossiblePrototype = function (it) {
        if (!isObject(it) && it !== null) {
            throw TypeError("Can't set " + String(it) + ' as a prototype');
        } return it;
    };

    /* eslint-disable no-proto -- safe */



    // `Object.setPrototypeOf` method
    // https://tc39.es/ecma262/#sec-object.setprototypeof
    // Works with __proto__ only. Old v8 can't work with null proto objects.
    var objectSetPrototypeOf = Object.setPrototypeOf || ('__proto__' in {} ? function () {
        var CORRECT_SETTER = false;
        var test = {};
        var setter;
        try {
            setter = Object.getOwnPropertyDescriptor(Object.prototype, '__proto__').set;
            setter.call(test, []);
            CORRECT_SETTER = test instanceof Array;
        } catch (error) { /* empty */ }
        return function setPrototypeOf(O, proto) {
            anObject(O);
            aPossiblePrototype(proto);
            if (CORRECT_SETTER) setter.call(O, proto);
            else O.__proto__ = proto;
            return O;
        };
    }() : undefined);

    // makes subclassing work correct for wrapped built-ins
    var inheritIfRequired = function ($this, dummy, Wrapper) {
        var NewTarget, NewTargetPrototype;
        if (
            // it can work only with native `setPrototypeOf`
            objectSetPrototypeOf &&
            // we haven't completely correct pre-ES6 way for getting `new.target`, so use this
            typeof (NewTarget = dummy.constructor) == 'function' &&
            NewTarget !== Wrapper &&
            isObject(NewTargetPrototype = NewTarget.prototype) &&
            NewTargetPrototype !== Wrapper.prototype
        ) objectSetPrototypeOf($this, NewTargetPrototype);
        return $this;
    };

    var collection = function (CONSTRUCTOR_NAME, wrapper, common) {
        var IS_MAP = CONSTRUCTOR_NAME.indexOf('Map') !== -1;
        var IS_WEAK = CONSTRUCTOR_NAME.indexOf('Weak') !== -1;
        var ADDER = IS_MAP ? 'set' : 'add';
        var NativeConstructor = global_1[CONSTRUCTOR_NAME];
        var NativePrototype = NativeConstructor && NativeConstructor.prototype;
        var Constructor = NativeConstructor;
        var exported = {};

        var fixMethod = function (KEY) {
            var nativeMethod = NativePrototype[KEY];
            redefine(NativePrototype, KEY,
                KEY == 'add' ? function add(value) {
                    nativeMethod.call(this, value === 0 ? 0 : value);
                    return this;
                } : KEY == 'delete' ? function (key) {
                    return IS_WEAK && !isObject(key) ? false : nativeMethod.call(this, key === 0 ? 0 : key);
                } : KEY == 'get' ? function get(key) {
                    return IS_WEAK && !isObject(key) ? undefined : nativeMethod.call(this, key === 0 ? 0 : key);
                } : KEY == 'has' ? function has(key) {
                    return IS_WEAK && !isObject(key) ? false : nativeMethod.call(this, key === 0 ? 0 : key);
                } : function set(key, value) {
                    nativeMethod.call(this, key === 0 ? 0 : key, value);
                    return this;
                }
            );
        };

        var REPLACE = isForced_1(
            CONSTRUCTOR_NAME,
            typeof NativeConstructor != 'function' || !(IS_WEAK || NativePrototype.forEach && !fails(function () {
                new NativeConstructor().entries().next();
            }))
        );

        if (REPLACE) {
            // create collection constructor
            Constructor = common.getConstructor(wrapper, CONSTRUCTOR_NAME, IS_MAP, ADDER);
            internalMetadata.REQUIRED = true;
        } else if (isForced_1(CONSTRUCTOR_NAME, true)) {
            var instance = new Constructor();
            // early implementations not supports chaining
            var HASNT_CHAINING = instance[ADDER](IS_WEAK ? {} : -0, 1) != instance;
            // V8 ~ Chromium 40- weak-collections throws on primitives, but should return false
            var THROWS_ON_PRIMITIVES = fails(function () { instance.has(1); });
            // most early implementations doesn't supports iterables, most modern - not close it correctly
            // eslint-disable-next-line no-new -- required for testing
            var ACCEPT_ITERABLES = checkCorrectnessOfIteration(function (iterable) { new NativeConstructor(iterable); });
            // for early implementations -0 and +0 not the same
            var BUGGY_ZERO = !IS_WEAK && fails(function () {
                // V8 ~ Chromium 42- fails only with 5+ elements
                var $instance = new NativeConstructor();
                var index = 5;
                while (index--) $instance[ADDER](index, index);
                return !$instance.has(-0);
            });

            if (!ACCEPT_ITERABLES) {
                Constructor = wrapper(function (dummy, iterable) {
                    anInstance(dummy, Constructor, CONSTRUCTOR_NAME);
                    var that = inheritIfRequired(new NativeConstructor(), dummy, Constructor);
                    if (iterable != undefined) iterate(iterable, that[ADDER], { that: that, AS_ENTRIES: IS_MAP });
                    return that;
                });
                Constructor.prototype = NativePrototype;
                NativePrototype.constructor = Constructor;
            }

            if (THROWS_ON_PRIMITIVES || BUGGY_ZERO) {
                fixMethod('delete');
                fixMethod('has');
                IS_MAP && fixMethod('get');
            }

            if (BUGGY_ZERO || HASNT_CHAINING) fixMethod(ADDER);

            // weak collections should not contains .clear method
            if (IS_WEAK && NativePrototype.clear) delete NativePrototype.clear;
        }

        exported[CONSTRUCTOR_NAME] = Constructor;
        _export({ global: true, forced: Constructor != NativeConstructor }, exported);

        setToStringTag(Constructor, CONSTRUCTOR_NAME);

        if (!IS_WEAK) common.setStrong(Constructor, CONSTRUCTOR_NAME, IS_MAP);

        return Constructor;
    };

    // `Object.keys` method
    // https://tc39.es/ecma262/#sec-object.keys
    var objectKeys = Object.keys || function keys(O) {
        return objectKeysInternal(O, enumBugKeys);
    };

    // `Object.defineProperties` method
    // https://tc39.es/ecma262/#sec-object.defineproperties
    var objectDefineProperties = descriptors ? Object.defineProperties : function defineProperties(O, Properties) {
        anObject(O);
        var keys = objectKeys(Properties);
        var length = keys.length;
        var index = 0;
        var key;
        while (length > index) objectDefineProperty.f(O, key = keys[index++], Properties[key]);
        return O;
    };

    var html = getBuiltIn('document', 'documentElement');

    var GT = '>';
    var LT = '<';
    var PROTOTYPE = 'prototype';
    var SCRIPT = 'script';
    var IE_PROTO$1 = sharedKey('IE_PROTO');

    var EmptyConstructor = function () { /* empty */ };

    var scriptTag = function (content) {
        return LT + SCRIPT + GT + content + LT + '/' + SCRIPT + GT;
    };

    // Create object with fake `null` prototype: use ActiveX Object with cleared prototype
    var NullProtoObjectViaActiveX = function (activeXDocument) {
        activeXDocument.write(scriptTag(''));
        activeXDocument.close();
        var temp = activeXDocument.parentWindow.Object;
        activeXDocument = null; // avoid memory leak
        return temp;
    };

    // Create object with fake `null` prototype: use iframe Object with cleared prototype
    var NullProtoObjectViaIFrame = function () {
        // Thrash, waste and sodomy: IE GC bug
        var iframe = documentCreateElement('iframe');
        var JS = 'java' + SCRIPT + ':';
        var iframeDocument;
        iframe.style.display = 'none';
        html.appendChild(iframe);
        // https://github.com/zloirock/core-js/issues/475
        iframe.src = String(JS);
        iframeDocument = iframe.contentWindow.document;
        iframeDocument.open();
        iframeDocument.write(scriptTag('document.F=Object'));
        iframeDocument.close();
        return iframeDocument.F;
    };

    // Check for document.domain and active x support
    // No need to use active x approach when document.domain is not set
    // see https://github.com/es-shims/es5-shim/issues/150
    // variation of https://github.com/kitcambridge/es5-shim/commit/4f738ac066346
    // avoid IE GC bug
    var activeXDocument;
    var NullProtoObject = function () {
        try {
            /* global ActiveXObject -- old IE */
            activeXDocument = document.domain && new ActiveXObject('htmlfile');
        } catch (error) { /* ignore */ }
        NullProtoObject = activeXDocument ? NullProtoObjectViaActiveX(activeXDocument) : NullProtoObjectViaIFrame();
        var length = enumBugKeys.length;
        while (length--) delete NullProtoObject[PROTOTYPE][enumBugKeys[length]];
        return NullProtoObject();
    };

    hiddenKeys$1[IE_PROTO$1] = true;

    // `Object.create` method
    // https://tc39.es/ecma262/#sec-object.create
    var objectCreate = Object.create || function create(O, Properties) {
        var result;
        if (O !== null) {
            EmptyConstructor[PROTOTYPE] = anObject(O);
            result = new EmptyConstructor();
            EmptyConstructor[PROTOTYPE] = null;
            // add "__proto__" for Object.getPrototypeOf polyfill
            result[IE_PROTO$1] = O;
        } else result = NullProtoObject();
        return Properties === undefined ? result : objectDefineProperties(result, Properties);
    };

    var redefineAll = function (target, src, options) {
        for (var key in src) redefine(target, key, src[key], options);
        return target;
    };

    // `ToObject` abstract operation
    // https://tc39.es/ecma262/#sec-toobject
    var toObject = function (argument) {
        return Object(requireObjectCoercible(argument));
    };

    var correctPrototypeGetter = !fails(function () {
        function F() { /* empty */ }
        F.prototype.constructor = null;
        return Object.getPrototypeOf(new F()) !== F.prototype;
    });

    var IE_PROTO = sharedKey('IE_PROTO');
    var ObjectPrototype = Object.prototype;

    // `Object.getPrototypeOf` method
    // https://tc39.es/ecma262/#sec-object.getprototypeof
    var objectGetPrototypeOf = correctPrototypeGetter ? Object.getPrototypeOf : function (O) {
        O = toObject(O);
        if (has$1(O, IE_PROTO)) return O[IE_PROTO];
        if (typeof O.constructor == 'function' && O instanceof O.constructor) {
            return O.constructor.prototype;
        } return O instanceof Object ? ObjectPrototype : null;
    };

    var ITERATOR$2 = wellKnownSymbol('iterator');
    var BUGGY_SAFARI_ITERATORS$1 = false;

    var returnThis$2 = function () { return this; };

    // `%IteratorPrototype%` object
    // https://tc39.es/ecma262/#sec-%iteratorprototype%-object
    var IteratorPrototype$2, PrototypeOfArrayIteratorPrototype, arrayIterator;

    if ([].keys) {
        arrayIterator = [].keys();
        // Safari 8 has buggy iterators w/o `next`
        if (!('next' in arrayIterator)) BUGGY_SAFARI_ITERATORS$1 = true;
        else {
            PrototypeOfArrayIteratorPrototype = objectGetPrototypeOf(objectGetPrototypeOf(arrayIterator));
            if (PrototypeOfArrayIteratorPrototype !== Object.prototype) IteratorPrototype$2 = PrototypeOfArrayIteratorPrototype;
        }
    }

    var NEW_ITERATOR_PROTOTYPE = IteratorPrototype$2 == undefined || fails(function () {
        var test = {};
        // FF44- legacy iterators case
        return IteratorPrototype$2[ITERATOR$2].call(test) !== test;
    });

    if (NEW_ITERATOR_PROTOTYPE) IteratorPrototype$2 = {};

    // 25.1.2.1.1 %IteratorPrototype%[@@iterator]()
    if (!has$1(IteratorPrototype$2, ITERATOR$2)) {
        createNonEnumerableProperty(IteratorPrototype$2, ITERATOR$2, returnThis$2);
    }

    var iteratorsCore = {
        IteratorPrototype: IteratorPrototype$2,
        BUGGY_SAFARI_ITERATORS: BUGGY_SAFARI_ITERATORS$1
    };

    var IteratorPrototype$1 = iteratorsCore.IteratorPrototype;





    var returnThis$1 = function () { return this; };

    var createIteratorConstructor = function (IteratorConstructor, NAME, next) {
        var TO_STRING_TAG = NAME + ' Iterator';
        IteratorConstructor.prototype = objectCreate(IteratorPrototype$1, { next: createPropertyDescriptor(1, next) });
        setToStringTag(IteratorConstructor, TO_STRING_TAG, false);
        iterators[TO_STRING_TAG] = returnThis$1;
        return IteratorConstructor;
    };

    var IteratorPrototype = iteratorsCore.IteratorPrototype;
    var BUGGY_SAFARI_ITERATORS = iteratorsCore.BUGGY_SAFARI_ITERATORS;
    var ITERATOR$1 = wellKnownSymbol('iterator');
    var KEYS = 'keys';
    var VALUES = 'values';
    var ENTRIES = 'entries';

    var returnThis = function () { return this; };

    var defineIterator = function (Iterable, NAME, IteratorConstructor, next, DEFAULT, IS_SET, FORCED) {
        createIteratorConstructor(IteratorConstructor, NAME, next);

        var getIterationMethod = function (KIND) {
            if (KIND === DEFAULT && defaultIterator) return defaultIterator;
            if (!BUGGY_SAFARI_ITERATORS && KIND in IterablePrototype) return IterablePrototype[KIND];
            switch (KIND) {
                case KEYS: return function keys() { return new IteratorConstructor(this, KIND); };
                case VALUES: return function values() { return new IteratorConstructor(this, KIND); };
                case ENTRIES: return function entries() { return new IteratorConstructor(this, KIND); };
            } return function () { return new IteratorConstructor(this); };
        };

        var TO_STRING_TAG = NAME + ' Iterator';
        var INCORRECT_VALUES_NAME = false;
        var IterablePrototype = Iterable.prototype;
        var nativeIterator = IterablePrototype[ITERATOR$1]
            || IterablePrototype['@@iterator']
            || DEFAULT && IterablePrototype[DEFAULT];
        var defaultIterator = !BUGGY_SAFARI_ITERATORS && nativeIterator || getIterationMethod(DEFAULT);
        var anyNativeIterator = NAME == 'Array' ? IterablePrototype.entries || nativeIterator : nativeIterator;
        var CurrentIteratorPrototype, methods, KEY;

        // fix native
        if (anyNativeIterator) {
            CurrentIteratorPrototype = objectGetPrototypeOf(anyNativeIterator.call(new Iterable()));
            if (IteratorPrototype !== Object.prototype && CurrentIteratorPrototype.next) {
                if (objectGetPrototypeOf(CurrentIteratorPrototype) !== IteratorPrototype) {
                    if (objectSetPrototypeOf) {
                        objectSetPrototypeOf(CurrentIteratorPrototype, IteratorPrototype);
                    } else if (typeof CurrentIteratorPrototype[ITERATOR$1] != 'function') {
                        createNonEnumerableProperty(CurrentIteratorPrototype, ITERATOR$1, returnThis);
                    }
                }
                // Set @@toStringTag to native iterators
                setToStringTag(CurrentIteratorPrototype, TO_STRING_TAG, true);
            }
        }

        // fix Array#{values, @@iterator}.name in V8 / FF
        if (DEFAULT == VALUES && nativeIterator && nativeIterator.name !== VALUES) {
            INCORRECT_VALUES_NAME = true;
            defaultIterator = function values() { return nativeIterator.call(this); };
        }

        // define iterator
        if (IterablePrototype[ITERATOR$1] !== defaultIterator) {
            createNonEnumerableProperty(IterablePrototype, ITERATOR$1, defaultIterator);
        }
        iterators[NAME] = defaultIterator;

        // export additional methods
        if (DEFAULT) {
            methods = {
                values: getIterationMethod(VALUES),
                keys: IS_SET ? defaultIterator : getIterationMethod(KEYS),
                entries: getIterationMethod(ENTRIES)
            };
            if (FORCED) for (KEY in methods) {
                if (BUGGY_SAFARI_ITERATORS || INCORRECT_VALUES_NAME || !(KEY in IterablePrototype)) {
                    redefine(IterablePrototype, KEY, methods[KEY]);
                }
            } else _export({ target: NAME, proto: true, forced: BUGGY_SAFARI_ITERATORS || INCORRECT_VALUES_NAME }, methods);
        }

        return methods;
    };

    var SPECIES$2 = wellKnownSymbol('species');

    var setSpecies = function (CONSTRUCTOR_NAME) {
        var Constructor = getBuiltIn(CONSTRUCTOR_NAME);
        var defineProperty = objectDefineProperty.f;

        if (descriptors && Constructor && !Constructor[SPECIES$2]) {
            defineProperty(Constructor, SPECIES$2, {
                configurable: true,
                get: function () { return this; }
            });
        }
    };

    var defineProperty = objectDefineProperty.f;








    var fastKey = internalMetadata.fastKey;


    var setInternalState$2 = internalState.set;
    var internalStateGetterFor = internalState.getterFor;

    var collectionStrong = {
        getConstructor: function (wrapper, CONSTRUCTOR_NAME, IS_MAP, ADDER) {
            var C = wrapper(function (that, iterable) {
                anInstance(that, C, CONSTRUCTOR_NAME);
                setInternalState$2(that, {
                    type: CONSTRUCTOR_NAME,
                    index: objectCreate(null),
                    first: undefined,
                    last: undefined,
                    size: 0
                });
                if (!descriptors) that.size = 0;
                if (iterable != undefined) iterate(iterable, that[ADDER], { that: that, AS_ENTRIES: IS_MAP });
            });

            var getInternalState = internalStateGetterFor(CONSTRUCTOR_NAME);

            var define = function (that, key, value) {
                var state = getInternalState(that);
                var entry = getEntry(that, key);
                var previous, index;
                // change existing entry
                if (entry) {
                    entry.value = value;
                    // create new entry
                } else {
                    state.last = entry = {
                        index: index = fastKey(key, true),
                        key: key,
                        value: value,
                        previous: previous = state.last,
                        next: undefined,
                        removed: false
                    };
                    if (!state.first) state.first = entry;
                    if (previous) previous.next = entry;
                    if (descriptors) state.size++;
                    else that.size++;
                    // add to index
                    if (index !== 'F') state.index[index] = entry;
                } return that;
            };

            var getEntry = function (that, key) {
                var state = getInternalState(that);
                // fast case
                var index = fastKey(key);
                var entry;
                if (index !== 'F') return state.index[index];
                // frozen object case
                for (entry = state.first; entry; entry = entry.next) {
                    if (entry.key == key) return entry;
                }
            };

            redefineAll(C.prototype, {
                // 23.1.3.1 Map.prototype.clear()
                // 23.2.3.2 Set.prototype.clear()
                clear: function clear() {
                    var that = this;
                    var state = getInternalState(that);
                    var data = state.index;
                    var entry = state.first;
                    while (entry) {
                        entry.removed = true;
                        if (entry.previous) entry.previous = entry.previous.next = undefined;
                        delete data[entry.index];
                        entry = entry.next;
                    }
                    state.first = state.last = undefined;
                    if (descriptors) state.size = 0;
                    else that.size = 0;
                },
                // 23.1.3.3 Map.prototype.delete(key)
                // 23.2.3.4 Set.prototype.delete(value)
                'delete': function (key) {
                    var that = this;
                    var state = getInternalState(that);
                    var entry = getEntry(that, key);
                    if (entry) {
                        var next = entry.next;
                        var prev = entry.previous;
                        delete state.index[entry.index];
                        entry.removed = true;
                        if (prev) prev.next = next;
                        if (next) next.previous = prev;
                        if (state.first == entry) state.first = next;
                        if (state.last == entry) state.last = prev;
                        if (descriptors) state.size--;
                        else that.size--;
                    } return !!entry;
                },
                // 23.2.3.6 Set.prototype.forEach(callbackfn, thisArg = undefined)
                // 23.1.3.5 Map.prototype.forEach(callbackfn, thisArg = undefined)
                forEach: function forEach(callbackfn /* , that = undefined */) {
                    var state = getInternalState(this);
                    var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
                    var entry;
                    while (entry = entry ? entry.next : state.first) {
                        boundFunction(entry.value, entry.key, this);
                        // revert to the last existing entry
                        while (entry && entry.removed) entry = entry.previous;
                    }
                },
                // 23.1.3.7 Map.prototype.has(key)
                // 23.2.3.7 Set.prototype.has(value)
                has: function has(key) {
                    return !!getEntry(this, key);
                }
            });

            redefineAll(C.prototype, IS_MAP ? {
                // 23.1.3.6 Map.prototype.get(key)
                get: function get(key) {
                    var entry = getEntry(this, key);
                    return entry && entry.value;
                },
                // 23.1.3.9 Map.prototype.set(key, value)
                set: function set(key, value) {
                    return define(this, key === 0 ? 0 : key, value);
                }
            } : {
                    // 23.2.3.1 Set.prototype.add(value)
                    add: function add(value) {
                        return define(this, value = value === 0 ? 0 : value, value);
                    }
                });
            if (descriptors) defineProperty(C.prototype, 'size', {
                get: function () {
                    return getInternalState(this).size;
                }
            });
            return C;
        },
        setStrong: function (C, CONSTRUCTOR_NAME, IS_MAP) {
            var ITERATOR_NAME = CONSTRUCTOR_NAME + ' Iterator';
            var getInternalCollectionState = internalStateGetterFor(CONSTRUCTOR_NAME);
            var getInternalIteratorState = internalStateGetterFor(ITERATOR_NAME);
            // add .keys, .values, .entries, [@@iterator]
            // 23.1.3.4, 23.1.3.8, 23.1.3.11, 23.1.3.12, 23.2.3.5, 23.2.3.8, 23.2.3.10, 23.2.3.11
            defineIterator(C, CONSTRUCTOR_NAME, function (iterated, kind) {
                setInternalState$2(this, {
                    type: ITERATOR_NAME,
                    target: iterated,
                    state: getInternalCollectionState(iterated),
                    kind: kind,
                    last: undefined
                });
            }, function () {
                var state = getInternalIteratorState(this);
                var kind = state.kind;
                var entry = state.last;
                // revert to the last existing entry
                while (entry && entry.removed) entry = entry.previous;
                // get next entry
                if (!state.target || !(state.last = entry = entry ? entry.next : state.state.first)) {
                    // or finish the iteration
                    state.target = undefined;
                    return { value: undefined, done: true };
                }
                // return step by kind
                if (kind == 'keys') return { value: entry.key, done: false };
                if (kind == 'values') return { value: entry.value, done: false };
                return { value: [entry.key, entry.value], done: false };
            }, IS_MAP ? 'entries' : 'values', !IS_MAP, true);

            // add [@@species], 23.1.2.2, 23.2.2.2
            setSpecies(CONSTRUCTOR_NAME);
        }
    };

    // `Set` constructor
    // https://tc39.es/ecma262/#sec-set-objects
    collection('Set', function (init) {
        return function Set() { return init(this, arguments.length ? arguments[0] : undefined); };
    }, collectionStrong);

    // `Object.prototype.toString` method implementation
    // https://tc39.es/ecma262/#sec-object.prototype.tostring
    var objectToString = toStringTagSupport ? {}.toString : function toString() {
        return '[object ' + classof(this) + ']';
    };

    // `Object.prototype.toString` method
    // https://tc39.es/ecma262/#sec-object.prototype.tostring
    if (!toStringTagSupport) {
        redefine(Object.prototype, 'toString', objectToString, { unsafe: true });
    }

    // `String.prototype.{ codePointAt, at }` methods implementation
    var createMethod$1 = function (CONVERT_TO_STRING) {
        return function ($this, pos) {
            var S = String(requireObjectCoercible($this));
            var position = toInteger(pos);
            var size = S.length;
            var first, second;
            if (position < 0 || position >= size) return CONVERT_TO_STRING ? '' : undefined;
            first = S.charCodeAt(position);
            return first < 0xD800 || first > 0xDBFF || position + 1 === size
                || (second = S.charCodeAt(position + 1)) < 0xDC00 || second > 0xDFFF
                ? CONVERT_TO_STRING ? S.charAt(position) : first
                : CONVERT_TO_STRING ? S.slice(position, position + 2) : (first - 0xD800 << 10) + (second - 0xDC00) + 0x10000;
        };
    };

    var stringMultibyte = {
        // `String.prototype.codePointAt` method
        // https://tc39.es/ecma262/#sec-string.prototype.codepointat
        codeAt: createMethod$1(false),
        // `String.prototype.at` method
        // https://github.com/mathiasbynens/String.prototype.at
        charAt: createMethod$1(true)
    };

    var charAt = stringMultibyte.charAt;



    var STRING_ITERATOR = 'String Iterator';
    var setInternalState$1 = internalState.set;
    var getInternalState$1 = internalState.getterFor(STRING_ITERATOR);

    // `String.prototype[@@iterator]` method
    // https://tc39.es/ecma262/#sec-string.prototype-@@iterator
    defineIterator(String, 'String', function (iterated) {
        setInternalState$1(this, {
            type: STRING_ITERATOR,
            string: String(iterated),
            index: 0
        });
        // `%StringIteratorPrototype%.next` method
        // https://tc39.es/ecma262/#sec-%stringiteratorprototype%.next
    }, function next() {
        var state = getInternalState$1(this);
        var string = state.string;
        var index = state.index;
        var point;
        if (index >= string.length) return { value: undefined, done: true };
        point = charAt(string, index);
        state.index += point.length;
        return { value: point, done: false };
    });

    // iterable DOM collections
    // flag - `iterable` interface - 'entries', 'keys', 'values', 'forEach' methods
    var domIterables = {
        CSSRuleList: 0,
        CSSStyleDeclaration: 0,
        CSSValueList: 0,
        ClientRectList: 0,
        DOMRectList: 0,
        DOMStringList: 0,
        DOMTokenList: 1,
        DataTransferItemList: 0,
        FileList: 0,
        HTMLAllCollection: 0,
        HTMLCollection: 0,
        HTMLFormElement: 0,
        HTMLSelectElement: 0,
        MediaList: 0,
        MimeTypeArray: 0,
        NamedNodeMap: 0,
        NodeList: 1,
        PaintRequestList: 0,
        Plugin: 0,
        PluginArray: 0,
        SVGLengthList: 0,
        SVGNumberList: 0,
        SVGPathSegList: 0,
        SVGPointList: 0,
        SVGStringList: 0,
        SVGTransformList: 0,
        SourceBufferList: 0,
        StyleSheetList: 0,
        TextTrackCueList: 0,
        TextTrackList: 0,
        TouchList: 0
    };

    var UNSCOPABLES = wellKnownSymbol('unscopables');
    var ArrayPrototype = Array.prototype;

    // Array.prototype[@@unscopables]
    // https://tc39.es/ecma262/#sec-array.prototype-@@unscopables
    if (ArrayPrototype[UNSCOPABLES] == undefined) {
        objectDefineProperty.f(ArrayPrototype, UNSCOPABLES, {
            configurable: true,
            value: objectCreate(null)
        });
    }

    // add a key to Array.prototype[@@unscopables]
    var addToUnscopables = function (key) {
        ArrayPrototype[UNSCOPABLES][key] = true;
    };

    var ARRAY_ITERATOR = 'Array Iterator';
    var setInternalState = internalState.set;
    var getInternalState = internalState.getterFor(ARRAY_ITERATOR);

    // `Array.prototype.entries` method
    // https://tc39.es/ecma262/#sec-array.prototype.entries
    // `Array.prototype.keys` method
    // https://tc39.es/ecma262/#sec-array.prototype.keys
    // `Array.prototype.values` method
    // https://tc39.es/ecma262/#sec-array.prototype.values
    // `Array.prototype[@@iterator]` method
    // https://tc39.es/ecma262/#sec-array.prototype-@@iterator
    // `CreateArrayIterator` internal method
    // https://tc39.es/ecma262/#sec-createarrayiterator
    var es_array_iterator = defineIterator(Array, 'Array', function (iterated, kind) {
        setInternalState(this, {
            type: ARRAY_ITERATOR,
            target: toIndexedObject(iterated), // target
            index: 0,                          // next index
            kind: kind                         // kind
        });
        // `%ArrayIteratorPrototype%.next` method
        // https://tc39.es/ecma262/#sec-%arrayiteratorprototype%.next
    }, function () {
        var state = getInternalState(this);
        var target = state.target;
        var kind = state.kind;
        var index = state.index++;
        if (!target || index >= target.length) {
            state.target = undefined;
            return { value: undefined, done: true };
        }
        if (kind == 'keys') return { value: index, done: false };
        if (kind == 'values') return { value: target[index], done: false };
        return { value: [index, target[index]], done: false };
    }, 'values');

    // argumentsList[@@iterator] is %ArrayProto_values%
    // https://tc39.es/ecma262/#sec-createunmappedargumentsobject
    // https://tc39.es/ecma262/#sec-createmappedargumentsobject
    iterators.Arguments = iterators.Array;

    // https://tc39.es/ecma262/#sec-array.prototype-@@unscopables
    addToUnscopables('keys');
    addToUnscopables('values');
    addToUnscopables('entries');

    var ITERATOR = wellKnownSymbol('iterator');
    var TO_STRING_TAG = wellKnownSymbol('toStringTag');
    var ArrayValues = es_array_iterator.values;

    for (var COLLECTION_NAME in domIterables) {
        var Collection = global_1[COLLECTION_NAME];
        var CollectionPrototype = Collection && Collection.prototype;
        if (CollectionPrototype) {
            // some Chrome versions have non-configurable methods on DOMTokenList
            if (CollectionPrototype[ITERATOR] !== ArrayValues) try {
                createNonEnumerableProperty(CollectionPrototype, ITERATOR, ArrayValues);
            } catch (error) {
                CollectionPrototype[ITERATOR] = ArrayValues;
            }
            if (!CollectionPrototype[TO_STRING_TAG]) {
                createNonEnumerableProperty(CollectionPrototype, TO_STRING_TAG, COLLECTION_NAME);
            }
            if (domIterables[COLLECTION_NAME]) for (var METHOD_NAME in es_array_iterator) {
                // some Chrome versions have non-configurable methods on DOMTokenList
                if (CollectionPrototype[METHOD_NAME] !== es_array_iterator[METHOD_NAME]) try {
                    createNonEnumerableProperty(CollectionPrototype, METHOD_NAME, es_array_iterator[METHOD_NAME]);
                } catch (error) {
                    CollectionPrototype[METHOD_NAME] = es_array_iterator[METHOD_NAME];
                }
            }
        }
    }

    path.Set;

    // https://tc39.github.io/proposal-setmap-offrom/




    var collectionFrom = function from(source /* , mapFn, thisArg */) {
        var length = arguments.length;
        var mapFn = length > 1 ? arguments[1] : undefined;
        var mapping, array, n, boundFunction;
        aFunction(this);
        mapping = mapFn !== undefined;
        if (mapping) aFunction(mapFn);
        if (source == undefined) return new this();
        array = [];
        if (mapping) {
            n = 0;
            boundFunction = functionBindContext(mapFn, length > 2 ? arguments[2] : undefined, 2);
            iterate(source, function (nextItem) {
                array.push(boundFunction(nextItem, n++));
            });
        } else {
            iterate(source, array.push, { that: array });
        }
        return new this(array);
    };

    // `Set.from` method
    // https://tc39.github.io/proposal-setmap-offrom/#sec-set.from
    _export({ target: 'Set', stat: true }, {
        from: collectionFrom
    });

    // https://tc39.github.io/proposal-setmap-offrom/
    var collectionOf = function of() {
        var length = arguments.length;
        var A = new Array(length);
        while (length--) A[length] = arguments[length];
        return new this(A);
    };

    // `Set.of` method
    // https://tc39.github.io/proposal-setmap-offrom/#sec-set.of
    _export({ target: 'Set', stat: true }, {
        of: collectionOf
    });

    // https://github.com/tc39/collection-methods
    var collectionAddAll = function (/* ...elements */) {
        var set = anObject(this);
        var adder = aFunction(set.add);
        for (var k = 0, len = arguments.length; k < len; k++) {
            adder.call(set, arguments[k]);
        }
        return set;
    };

    // `Set.prototype.addAll` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        addAll: function addAll(/* ...elements */) {
            return collectionAddAll.apply(this, arguments);
        }
    });

    // https://github.com/tc39/collection-methods
    var collectionDeleteAll = function (/* ...elements */) {
        var collection = anObject(this);
        var remover = aFunction(collection['delete']);
        var allDeleted = true;
        var wasDeleted;
        for (var k = 0, len = arguments.length; k < len; k++) {
            wasDeleted = remover.call(collection, arguments[k]);
            allDeleted = allDeleted && wasDeleted;
        }
        return !!allDeleted;
    };

    // `Set.prototype.deleteAll` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        deleteAll: function deleteAll(/* ...elements */) {
            return collectionDeleteAll.apply(this, arguments);
        }
    });

    var getIterator = function (it) {
        var iteratorMethod = getIteratorMethod(it);
        if (typeof iteratorMethod != 'function') {
            throw TypeError(String(it) + ' is not iterable');
        } return anObject(iteratorMethod.call(it));
    };

    var getSetIterator = function (it) {
        // eslint-disable-next-line no-undef -- safe
        return Set.prototype.values.call(it);
    };

    // `Set.prototype.every` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        every: function every(callbackfn /* , thisArg */) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return !iterate(iterator, function (value, stop) {
                if (!boundFunction(value, value, set)) return stop();
            }, { IS_ITERATOR: true, INTERRUPTED: true }).stopped;
        }
    });

    var SPECIES$1 = wellKnownSymbol('species');

    // `SpeciesConstructor` abstract operation
    // https://tc39.es/ecma262/#sec-speciesconstructor
    var speciesConstructor = function (O, defaultConstructor) {
        var C = anObject(O).constructor;
        var S;
        return C === undefined || (S = anObject(C)[SPECIES$1]) == undefined ? defaultConstructor : aFunction(S);
    };

    // `Set.prototype.difference` method
    // https://github.com/tc39/proposal-set-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        difference: function difference(iterable) {
            var set = anObject(this);
            var newSet = new (speciesConstructor(set, getBuiltIn('Set')))(set);
            var remover = aFunction(newSet['delete']);
            iterate(iterable, function (value) {
                remover.call(newSet, value);
            });
            return newSet;
        }
    });

    // `Set.prototype.filter` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        filter: function filter(callbackfn /* , thisArg */) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            var newSet = new (speciesConstructor(set, getBuiltIn('Set')))();
            var adder = aFunction(newSet.add);
            iterate(iterator, function (value) {
                if (boundFunction(value, value, set)) adder.call(newSet, value);
            }, { IS_ITERATOR: true });
            return newSet;
        }
    });

    // `Set.prototype.find` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        find: function find(callbackfn /* , thisArg */) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return iterate(iterator, function (value, stop) {
                if (boundFunction(value, value, set)) return stop(value);
            }, { IS_ITERATOR: true, INTERRUPTED: true }).result;
        }
    });

    // `Set.prototype.intersection` method
    // https://github.com/tc39/proposal-set-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        intersection: function intersection(iterable) {
            var set = anObject(this);
            var newSet = new (speciesConstructor(set, getBuiltIn('Set')))();
            var hasCheck = aFunction(set.has);
            var adder = aFunction(newSet.add);
            iterate(iterable, function (value) {
                if (hasCheck.call(set, value)) adder.call(newSet, value);
            });
            return newSet;
        }
    });

    // `Set.prototype.isDisjointFrom` method
    // https://tc39.github.io/proposal-set-methods/#Set.prototype.isDisjointFrom
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        isDisjointFrom: function isDisjointFrom(iterable) {
            var set = anObject(this);
            var hasCheck = aFunction(set.has);
            return !iterate(iterable, function (value, stop) {
                if (hasCheck.call(set, value) === true) return stop();
            }, { INTERRUPTED: true }).stopped;
        }
    });

    // `Set.prototype.isSubsetOf` method
    // https://tc39.github.io/proposal-set-methods/#Set.prototype.isSubsetOf
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        isSubsetOf: function isSubsetOf(iterable) {
            var iterator = getIterator(this);
            var otherSet = anObject(iterable);
            var hasCheck = otherSet.has;
            if (typeof hasCheck != 'function') {
                otherSet = new (getBuiltIn('Set'))(iterable);
                hasCheck = aFunction(otherSet.has);
            }
            return !iterate(iterator, function (value, stop) {
                if (hasCheck.call(otherSet, value) === false) return stop();
            }, { IS_ITERATOR: true, INTERRUPTED: true }).stopped;
        }
    });

    // `Set.prototype.isSupersetOf` method
    // https://tc39.github.io/proposal-set-methods/#Set.prototype.isSupersetOf
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        isSupersetOf: function isSupersetOf(iterable) {
            var set = anObject(this);
            var hasCheck = aFunction(set.has);
            return !iterate(iterable, function (value, stop) {
                if (hasCheck.call(set, value) === false) return stop();
            }, { INTERRUPTED: true }).stopped;
        }
    });

    // `Set.prototype.join` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        join: function join(separator) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var sep = separator === undefined ? ',' : String(separator);
            var result = [];
            iterate(iterator, result.push, { that: result, IS_ITERATOR: true });
            return result.join(sep);
        }
    });

    // `Set.prototype.map` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        map: function map(callbackfn /* , thisArg */) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            var newSet = new (speciesConstructor(set, getBuiltIn('Set')))();
            var adder = aFunction(newSet.add);
            iterate(iterator, function (value) {
                adder.call(newSet, boundFunction(value, value, set));
            }, { IS_ITERATOR: true });
            return newSet;
        }
    });

    // `Set.prototype.reduce` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        reduce: function reduce(callbackfn /* , initialValue */) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var noInitial = arguments.length < 2;
            var accumulator = noInitial ? undefined : arguments[1];
            aFunction(callbackfn);
            iterate(iterator, function (value) {
                if (noInitial) {
                    noInitial = false;
                    accumulator = value;
                } else {
                    accumulator = callbackfn(accumulator, value, value, set);
                }
            }, { IS_ITERATOR: true });
            if (noInitial) throw TypeError('Reduce of empty set with no initial value');
            return accumulator;
        }
    });

    // `Set.prototype.some` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        some: function some(callbackfn /* , thisArg */) {
            var set = anObject(this);
            var iterator = getSetIterator(set);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return iterate(iterator, function (value, stop) {
                if (boundFunction(value, value, set)) return stop();
            }, { IS_ITERATOR: true, INTERRUPTED: true }).stopped;
        }
    });

    // `Set.prototype.symmetricDifference` method
    // https://github.com/tc39/proposal-set-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        symmetricDifference: function symmetricDifference(iterable) {
            var set = anObject(this);
            var newSet = new (speciesConstructor(set, getBuiltIn('Set')))(set);
            var remover = aFunction(newSet['delete']);
            var adder = aFunction(newSet.add);
            iterate(iterable, function (value) {
                remover.call(newSet, value) || adder.call(newSet, value);
            });
            return newSet;
        }
    });

    // `Set.prototype.union` method
    // https://github.com/tc39/proposal-set-methods
    _export({ target: 'Set', proto: true, real: true, forced: isPure }, {
        union: function union(iterable) {
            var set = anObject(this);
            var newSet = new (speciesConstructor(set, getBuiltIn('Set')))(set);
            iterate(iterable, aFunction(newSet.add), { that: newSet });
            return newSet;
        }
    });

    // `Map` constructor
    // https://tc39.es/ecma262/#sec-map-objects
    collection('Map', function (init) {
        return function Map() { return init(this, arguments.length ? arguments[0] : undefined); };
    }, collectionStrong);

    path.Map;

    // `Map.from` method
    // https://tc39.github.io/proposal-setmap-offrom/#sec-map.from
    _export({ target: 'Map', stat: true }, {
        from: collectionFrom
    });

    // `Map.of` method
    // https://tc39.github.io/proposal-setmap-offrom/#sec-map.of
    _export({ target: 'Map', stat: true }, {
        of: collectionOf
    });

    // `Map.prototype.deleteAll` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        deleteAll: function deleteAll(/* ...elements */) {
            return collectionDeleteAll.apply(this, arguments);
        }
    });

    // `Map.prototype.emplace` method
    // https://github.com/thumbsupep/proposal-upsert
    var mapEmplace = function emplace(key, handler) {
        var map = anObject(this);
        var value = (map.has(key) && 'update' in handler)
            ? handler.update(map.get(key), key, map)
            : handler.insert(key, map);
        map.set(key, value);
        return value;
    };

    // `Map.prototype.emplace` method
    // https://github.com/thumbsupep/proposal-upsert
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        emplace: mapEmplace
    });

    var getMapIterator = function (it) {
        // eslint-disable-next-line no-undef -- safe
        return Map.prototype.entries.call(it);
    };

    // `Map.prototype.every` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        every: function every(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return !iterate(iterator, function (key, value, stop) {
                if (!boundFunction(value, key, map)) return stop();
            }, { AS_ENTRIES: true, IS_ITERATOR: true, INTERRUPTED: true }).stopped;
        }
    });

    // `Map.prototype.filter` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        filter: function filter(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            var newMap = new (speciesConstructor(map, getBuiltIn('Map')))();
            var setter = aFunction(newMap.set);
            iterate(iterator, function (key, value) {
                if (boundFunction(value, key, map)) setter.call(newMap, key, value);
            }, { AS_ENTRIES: true, IS_ITERATOR: true });
            return newMap;
        }
    });

    // `Map.prototype.find` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        find: function find(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return iterate(iterator, function (key, value, stop) {
                if (boundFunction(value, key, map)) return stop(value);
            }, { AS_ENTRIES: true, IS_ITERATOR: true, INTERRUPTED: true }).result;
        }
    });

    // `Map.prototype.findKey` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        findKey: function findKey(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return iterate(iterator, function (key, value, stop) {
                if (boundFunction(value, key, map)) return stop(key);
            }, { AS_ENTRIES: true, IS_ITERATOR: true, INTERRUPTED: true }).result;
        }
    });

    // `Map.groupBy` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', stat: true }, {
        groupBy: function groupBy(iterable, keyDerivative) {
            var newMap = new this();
            aFunction(keyDerivative);
            var has = aFunction(newMap.has);
            var get = aFunction(newMap.get);
            var set = aFunction(newMap.set);
            iterate(iterable, function (element) {
                var derivedKey = keyDerivative(element);
                if (!has.call(newMap, derivedKey)) set.call(newMap, derivedKey, [element]);
                else get.call(newMap, derivedKey).push(element);
            });
            return newMap;
        }
    });

    // `SameValueZero` abstract operation
    // https://tc39.es/ecma262/#sec-samevaluezero
    var sameValueZero = function (x, y) {
        // eslint-disable-next-line no-self-compare -- NaN check
        return x === y || x != x && y != y;
    };

    // `Map.prototype.includes` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        includes: function includes(searchElement) {
            return iterate(getMapIterator(anObject(this)), function (key, value, stop) {
                if (sameValueZero(value, searchElement)) return stop();
            }, { AS_ENTRIES: true, IS_ITERATOR: true, INTERRUPTED: true }).stopped;
        }
    });

    // `Map.keyBy` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', stat: true }, {
        keyBy: function keyBy(iterable, keyDerivative) {
            var newMap = new this();
            aFunction(keyDerivative);
            var setter = aFunction(newMap.set);
            iterate(iterable, function (element) {
                setter.call(newMap, keyDerivative(element), element);
            });
            return newMap;
        }
    });

    // `Map.prototype.includes` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        keyOf: function keyOf(searchElement) {
            return iterate(getMapIterator(anObject(this)), function (key, value, stop) {
                if (value === searchElement) return stop(key);
            }, { AS_ENTRIES: true, IS_ITERATOR: true, INTERRUPTED: true }).result;
        }
    });

    // `Map.prototype.mapKeys` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        mapKeys: function mapKeys(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            var newMap = new (speciesConstructor(map, getBuiltIn('Map')))();
            var setter = aFunction(newMap.set);
            iterate(iterator, function (key, value) {
                setter.call(newMap, boundFunction(value, key, map), value);
            }, { AS_ENTRIES: true, IS_ITERATOR: true });
            return newMap;
        }
    });

    // `Map.prototype.mapValues` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        mapValues: function mapValues(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            var newMap = new (speciesConstructor(map, getBuiltIn('Map')))();
            var setter = aFunction(newMap.set);
            iterate(iterator, function (key, value) {
                setter.call(newMap, key, boundFunction(value, key, map));
            }, { AS_ENTRIES: true, IS_ITERATOR: true });
            return newMap;
        }
    });

    // `Map.prototype.merge` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        // eslint-disable-next-line no-unused-vars -- required for `.length`
        merge: function merge(iterable /* ...iterbles */) {
            var map = anObject(this);
            var setter = aFunction(map.set);
            var i = 0;
            while (i < arguments.length) {
                iterate(arguments[i++], setter, { that: map, AS_ENTRIES: true });
            }
            return map;
        }
    });

    // `Map.prototype.reduce` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        reduce: function reduce(callbackfn /* , initialValue */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var noInitial = arguments.length < 2;
            var accumulator = noInitial ? undefined : arguments[1];
            aFunction(callbackfn);
            iterate(iterator, function (key, value) {
                if (noInitial) {
                    noInitial = false;
                    accumulator = value;
                } else {
                    accumulator = callbackfn(accumulator, value, key, map);
                }
            }, { AS_ENTRIES: true, IS_ITERATOR: true });
            if (noInitial) throw TypeError('Reduce of empty map with no initial value');
            return accumulator;
        }
    });

    // `Set.prototype.some` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        some: function some(callbackfn /* , thisArg */) {
            var map = anObject(this);
            var iterator = getMapIterator(map);
            var boundFunction = functionBindContext(callbackfn, arguments.length > 1 ? arguments[1] : undefined, 3);
            return iterate(iterator, function (key, value, stop) {
                if (boundFunction(value, key, map)) return stop();
            }, { AS_ENTRIES: true, IS_ITERATOR: true, INTERRUPTED: true }).stopped;
        }
    });

    // `Set.prototype.update` method
    // https://github.com/tc39/proposal-collection-methods
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        update: function update(key, callback /* , thunk */) {
            var map = anObject(this);
            var length = arguments.length;
            aFunction(callback);
            var isPresentInMap = map.has(key);
            if (!isPresentInMap && length < 3) {
                throw TypeError('Updating absent value');
            }
            var value = isPresentInMap ? map.get(key) : aFunction(length > 2 ? arguments[2] : undefined)(key, map);
            map.set(key, callback(value, key, map));
            return map;
        }
    });

    // `Map.prototype.upsert` method
    // https://github.com/thumbsupep/proposal-upsert
    var mapUpsert = function upsert(key, updateFn /* , insertFn */) {
        var map = anObject(this);
        var insertFn = arguments.length > 2 ? arguments[2] : undefined;
        var value;
        if (typeof updateFn != 'function' && typeof insertFn != 'function') {
            throw TypeError('At least one callback required');
        }
        if (map.has(key)) {
            value = map.get(key);
            if (typeof updateFn == 'function') {
                value = updateFn(value);
                map.set(key, value);
            }
        } else if (typeof insertFn == 'function') {
            value = insertFn();
            map.set(key, value);
        } return value;
    };

    // TODO: remove from `core-js@4`




    // `Map.prototype.upsert` method (replaced by `Map.prototype.emplace`)
    // https://github.com/thumbsupep/proposal-upsert
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        upsert: mapUpsert
    });

    // TODO: remove from `core-js@4`




    // `Map.prototype.updateOrInsert` method (replaced by `Map.prototype.emplace`)
    // https://github.com/thumbsupep/proposal-upsert
    _export({ target: 'Map', proto: true, real: true, forced: isPure }, {
        updateOrInsert: mapUpsert
    });

    // `IsArray` abstract operation
    // https://tc39.es/ecma262/#sec-isarray
    var isArray = Array.isArray || function isArray(arg) {
        return classofRaw(arg) == 'Array';
    };

    var SPECIES = wellKnownSymbol('species');

    // `ArraySpeciesCreate` abstract operation
    // https://tc39.es/ecma262/#sec-arrayspeciescreate
    var arraySpeciesCreate = function (originalArray, length) {
        var C;
        if (isArray(originalArray)) {
            C = originalArray.constructor;
            // cross-realm fallback
            if (typeof C == 'function' && (C === Array || isArray(C.prototype))) C = undefined;
            else if (isObject(C)) {
                C = C[SPECIES];
                if (C === null) C = undefined;
            }
        } return new (C === undefined ? Array : C)(length === 0 ? 0 : length);
    };

    var push = [].push;

    // `Array.prototype.{ forEach, map, filter, some, every, find, findIndex, filterOut }` methods implementation
    var createMethod = function (TYPE) {
        var IS_MAP = TYPE == 1;
        var IS_FILTER = TYPE == 2;
        var IS_SOME = TYPE == 3;
        var IS_EVERY = TYPE == 4;
        var IS_FIND_INDEX = TYPE == 6;
        var IS_FILTER_OUT = TYPE == 7;
        var NO_HOLES = TYPE == 5 || IS_FIND_INDEX;
        return function ($this, callbackfn, that, specificCreate) {
            var O = toObject($this);
            var self = indexedObject(O);
            var boundFunction = functionBindContext(callbackfn, that, 3);
            var length = toLength(self.length);
            var index = 0;
            var create = specificCreate || arraySpeciesCreate;
            var target = IS_MAP ? create($this, length) : IS_FILTER || IS_FILTER_OUT ? create($this, 0) : undefined;
            var value, result;
            for (; length > index; index++) if (NO_HOLES || index in self) {
                value = self[index];
                result = boundFunction(value, index, O);
                if (TYPE) {
                    if (IS_MAP) target[index] = result; // map
                    else if (result) switch (TYPE) {
                        case 3: return true;              // some
                        case 5: return value;             // find
                        case 6: return index;             // findIndex
                        case 2: push.call(target, value); // filter
                    } else switch (TYPE) {
                        case 4: return false;             // every
                        case 7: push.call(target, value); // filterOut
                    }
                }
            }
            return IS_FIND_INDEX ? -1 : IS_SOME || IS_EVERY ? IS_EVERY : target;
        };
    };

    var arrayIteration = {
        // `Array.prototype.forEach` method
        // https://tc39.es/ecma262/#sec-array.prototype.foreach
        forEach: createMethod(0),
        // `Array.prototype.map` method
        // https://tc39.es/ecma262/#sec-array.prototype.map
        map: createMethod(1),
        // `Array.prototype.filter` method
        // https://tc39.es/ecma262/#sec-array.prototype.filter
        filter: createMethod(2),
        // `Array.prototype.some` method
        // https://tc39.es/ecma262/#sec-array.prototype.some
        some: createMethod(3),
        // `Array.prototype.every` method
        // https://tc39.es/ecma262/#sec-array.prototype.every
        every: createMethod(4),
        // `Array.prototype.find` method
        // https://tc39.es/ecma262/#sec-array.prototype.find
        find: createMethod(5),
        // `Array.prototype.findIndex` method
        // https://tc39.es/ecma262/#sec-array.prototype.findIndex
        findIndex: createMethod(6),
        // `Array.prototype.filterOut` method
        // https://github.com/tc39/proposal-array-filtering
        filterOut: createMethod(7)
    };

    var $find = arrayIteration.find;


    var FIND = 'find';
    var SKIPS_HOLES = true;

    // Shouldn't skip holes
    if (FIND in []) Array(1)[FIND](function () { SKIPS_HOLES = false; });

    // `Array.prototype.find` method
    // https://tc39.es/ecma262/#sec-array.prototype.find
    _export({ target: 'Array', proto: true, forced: SKIPS_HOLES }, {
        find: function find(callbackfn /* , that = undefined */) {
            return $find(this, callbackfn, arguments.length > 1 ? arguments[1] : undefined);
        }
    });

    // https://tc39.es/ecma262/#sec-array.prototype-@@unscopables
    addToUnscopables(FIND);

    var call = Function.call;

    var entryUnbind = function (CONSTRUCTOR, METHOD, length) {
        return functionBindContext(call, global_1[CONSTRUCTOR].prototype[METHOD], length);
    };

    entryUnbind('Array', 'find');

    /**
     * returns true if the given object is a promise
     */
    function isPromise(obj) {
        if (obj && typeof obj.then === 'function') {
            return true;
        } else {
            return false;
        }
    }
    function sleep(time) {
        if (!time) time = 0;
        return new Promise(function (res) {
            return setTimeout(res, time);
        });
    }
    function randomInt(min, max) {
        return Math.floor(Math.random() * (max - min + 1) + min);
    }
    /**
     * https://stackoverflow.com/a/8084248
     */

    function randomToken() {
        return Math.random().toString(36).substring(2);
    }
    var lastMs = 0;
    var additional = 0;
    /**
     * returns the current time in micro-seconds,
     * WARNING: This is a pseudo-function
     * Performance.now is not reliable in webworkers, so we just make sure to never return the same time.
     * This is enough in browsers, and this function will not be used in nodejs.
     * The main reason for this hack is to ensure that BroadcastChannel behaves equal to production when it is used in fast-running unit tests.
     */

    function microSeconds$4() {
        var ms = new Date().getTime();

        if (ms === lastMs) {
            additional++;
            return ms * 1000 + additional;
        } else {
            lastMs = ms;
            additional = 0;
            return ms * 1000;
        }
    }
    /**
     * copied from the 'detect-node' npm module
     * We cannot use the module directly because it causes problems with rollup
     * @link https://github.com/iliakan/detect-node/blob/master/index.js
     */

    var isNode = Object.prototype.toString.call(typeof process !== 'undefined' ? process : 0) === '[object process]';

    var microSeconds$3 = microSeconds$4;
    var type$3 = 'native';
    function create$3(channelName) {
        var state = {
            messagesCallback: null,
            bc: new BroadcastChannel(channelName),
            subFns: [] // subscriberFunctions

        };

        state.bc.onmessage = function (msg) {
            if (state.messagesCallback) {
                state.messagesCallback(msg.data);
            }
        };

        return state;
    }
    function close$3(channelState) {
        channelState.bc.close();
        channelState.subFns = [];
    }
    function postMessage$3(channelState, messageJson) {
        channelState.bc.postMessage(messageJson, false);
    }
    function onMessage$3(channelState, fn) {
        channelState.messagesCallback = fn;
    }
    function canBeUsed$3() {
        /**
         * in the electron-renderer, isNode will be true even if we are in browser-context
         * so we also check if window is undefined
         */
        if (isNode && typeof window === 'undefined') return false;

        if (typeof BroadcastChannel === 'function') {
            if (BroadcastChannel._pubkey) {
                throw new Error('BroadcastChannel: Do not overwrite window.BroadcastChannel with this module, this is not a polyfill');
            }

            return true;
        } else return false;
    }
    function averageResponseTime$3() {
        return 150;
    }
    var NativeMethod = {
        create: create$3,
        close: close$3,
        onMessage: onMessage$3,
        postMessage: postMessage$3,
        canBeUsed: canBeUsed$3,
        type: type$3,
        averageResponseTime: averageResponseTime$3,
        microSeconds: microSeconds$3
    };

    /**
     * this is a set which automatically forgets
     * a given entry when a new entry is set and the ttl
     * of the old one is over
     * @constructor
     */
    var ObliviousSet = function ObliviousSet(ttl) {
        var set = new Set();
        var timeMap = new Map();
        this.has = set.has.bind(set);

        this.add = function (value) {
            timeMap.set(value, now());
            set.add(value);

            _removeTooOldValues();
        };

        this.clear = function () {
            set.clear();
            timeMap.clear();
        };

        function _removeTooOldValues() {
            var olderThen = now() - ttl;
            var iterator = set[Symbol.iterator]();

            while (true) {
                var value = iterator.next().value;
                if (!value) return; // no more elements

                var time = timeMap.get(value);

                if (time < olderThen) {
                    timeMap["delete"](value);
                    set["delete"](value);
                } else {
                    // we reached a value that is not old enough
                    return;
                }
            }
        }
    };

    function now() {
        return new Date().getTime();
    }

    function fillOptionsWithDefaults$1() {
        var originalOptions = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : {};
        var options = JSON.parse(JSON.stringify(originalOptions)); // main

        if (typeof options.webWorkerSupport === 'undefined') options.webWorkerSupport = true; // indexed-db

        if (!options.idb) options.idb = {}; //  after this time the messages get deleted

        if (!options.idb.ttl) options.idb.ttl = 1000 * 45;
        if (!options.idb.fallbackInterval) options.idb.fallbackInterval = 150; //  handles abrupt db onclose events.

        if (originalOptions.idb && typeof originalOptions.idb.onclose === 'function') options.idb.onclose = originalOptions.idb.onclose; // localstorage

        if (!options.localstorage) options.localstorage = {};
        if (!options.localstorage.removeTimeout) options.localstorage.removeTimeout = 1000 * 60; // custom methods

        if (originalOptions.methods) options.methods = originalOptions.methods; // node

        if (!options.node) options.node = {};
        if (!options.node.ttl) options.node.ttl = 1000 * 60 * 2; // 2 minutes;

        if (typeof options.node.useFastPath === 'undefined') options.node.useFastPath = true;
        return options;
    }

    /**
     * this method uses indexeddb to store the messages
     * There is currently no observerAPI for idb
     * @link https://github.com/w3c/IndexedDB/issues/51
     */
    var microSeconds$2 = microSeconds$4;
    var DB_PREFIX = 'pubkey.broadcast-channel-0-';
    var OBJECT_STORE_ID = 'messages';
    var type$2 = 'idb';
    function getIdb() {
        if (typeof indexedDB !== 'undefined') return indexedDB;

        if (typeof window !== 'undefined') {
            if (typeof window.mozIndexedDB !== 'undefined') return window.mozIndexedDB;
            if (typeof window.webkitIndexedDB !== 'undefined') return window.webkitIndexedDB;
            if (typeof window.msIndexedDB !== 'undefined') return window.msIndexedDB;
        }

        return false;
    }
    function createDatabase(channelName) {
        var IndexedDB = getIdb(); // create table

        var dbName = DB_PREFIX + channelName;
        var openRequest = IndexedDB.open(dbName, 1);

        openRequest.onupgradeneeded = function (ev) {
            var db = ev.target.result;
            db.createObjectStore(OBJECT_STORE_ID, {
                keyPath: 'id',
                autoIncrement: true
            });
        };

        var dbPromise = new Promise(function (res, rej) {
            openRequest.onerror = function (ev) {
                return rej(ev);
            };

            openRequest.onsuccess = function () {
                res(openRequest.result);
            };
        });
        return dbPromise;
    }
    /**
     * writes the new message to the database
     * so other readers can find it
     */

    function writeMessage(db, readerUuid, messageJson) {
        var time = new Date().getTime();
        var writeObject = {
            uuid: readerUuid,
            time: time,
            data: messageJson
        };
        var transaction = db.transaction([OBJECT_STORE_ID], 'readwrite');
        return new Promise(function (res, rej) {
            transaction.oncomplete = function () {
                return res();
            };

            transaction.onerror = function (ev) {
                return rej(ev);
            };

            var objectStore = transaction.objectStore(OBJECT_STORE_ID);
            objectStore.add(writeObject);
        });
    }
    function getMessagesHigherThan(db, lastCursorId) {
        var objectStore = db.transaction(OBJECT_STORE_ID).objectStore(OBJECT_STORE_ID);
        var ret = [];

        function openCursor() {
            // Occasionally Safari will fail on IDBKeyRange.bound, this
            // catches that error, having it open the cursor to the first
            // item. When it gets data it will advance to the desired key.
            try {
                var keyRangeValue = IDBKeyRange.bound(lastCursorId + 1, Infinity);
                return objectStore.openCursor(keyRangeValue);
            } catch (e) {
                return objectStore.openCursor();
            }
        }

        return new Promise(function (res) {
            openCursor().onsuccess = function (ev) {
                var cursor = ev.target.result;

                if (cursor) {
                    if (cursor.value.id < lastCursorId + 1) {
                        cursor["continue"](lastCursorId + 1);
                    } else {
                        ret.push(cursor.value);
                        cursor["continue"]();
                    }
                } else {
                    res(ret);
                }
            };
        });
    }
    function removeMessageById(db, id) {
        var request = db.transaction([OBJECT_STORE_ID], 'readwrite').objectStore(OBJECT_STORE_ID)["delete"](id);
        return new Promise(function (res) {
            request.onsuccess = function () {
                return res();
            };
        });
    }
    function getOldMessages(db, ttl) {
        var olderThen = new Date().getTime() - ttl;
        var objectStore = db.transaction(OBJECT_STORE_ID).objectStore(OBJECT_STORE_ID);
        var ret = [];
        return new Promise(function (res) {
            objectStore.openCursor().onsuccess = function (ev) {
                var cursor = ev.target.result;

                if (cursor) {
                    var msgObk = cursor.value;

                    if (msgObk.time < olderThen) {
                        ret.push(msgObk); //alert("Name for SSN " + cursor.key + " is " + cursor.value.name);

                        cursor["continue"]();
                    } else {
                        // no more old messages,
                        res(ret);
                        return;
                    }
                } else {
                    res(ret);
                }
            };
        });
    }
    function cleanOldMessages(db, ttl) {
        return getOldMessages(db, ttl).then(function (tooOld) {
            return Promise.all(tooOld.map(function (msgObj) {
                return removeMessageById(db, msgObj.id);
            }));
        });
    }
    function create$2(channelName, options) {
        options = fillOptionsWithDefaults$1(options);
        return createDatabase(channelName).then(function (db) {
            var state = {
                closed: false,
                lastCursorId: 0,
                channelName: channelName,
                options: options,
                uuid: randomToken(),

                /**
                 * emittedMessagesIds
                 * contains all messages that have been emitted before
                 * @type {ObliviousSet}
                 */
                eMIs: new ObliviousSet(options.idb.ttl * 2),
                // ensures we do not read messages in parrallel
                writeBlockPromise: Promise.resolve(),
                messagesCallback: null,
                readQueuePromises: [],
                db: db
            };
            /**
             * Handle abrupt closes that do not originate from db.close().
             * This could happen, for example, if the underlying storage is
             * removed or if the user clears the database in the browser's
             * history preferences.
             */

            db.onclose = function () {
                state.closed = true;
                if (options.idb.onclose) options.idb.onclose();
            };
            /**
             * if service-workers are used,
             * we have no 'storage'-event if they post a message,
             * therefore we also have to set an interval
             */


            _readLoop(state);

            return state;
        });
    }

    function _readLoop(state) {
        if (state.closed) return;
        readNewMessages(state).then(function () {
            return sleep(state.options.idb.fallbackInterval);
        }).then(function () {
            return _readLoop(state);
        });
    }

    function _filterMessage(msgObj, state) {
        if (msgObj.uuid === state.uuid) return false; // send by own

        if (state.eMIs.has(msgObj.id)) return false; // already emitted

        if (msgObj.data.time < state.messagesCallbackTime) return false; // older then onMessageCallback

        return true;
    }
    /**
     * reads all new messages from the database and emits them
     */


    function readNewMessages(state) {
        // channel already closed
        if (state.closed) return Promise.resolve(); // if no one is listening, we do not need to scan for new messages

        if (!state.messagesCallback) return Promise.resolve();
        return getMessagesHigherThan(state.db, state.lastCursorId).then(function (newerMessages) {
            var useMessages = newerMessages
                /**
                 * there is a bug in iOS where the msgObj can be undefined some times
                 * so we filter them out
                 * @link https://github.com/pubkey/broadcast-channel/issues/19
                 */
                .filter(function (msgObj) {
                    return !!msgObj;
                }).map(function (msgObj) {
                    if (msgObj.id > state.lastCursorId) {
                        state.lastCursorId = msgObj.id;
                    }

                    return msgObj;
                }).filter(function (msgObj) {
                    return _filterMessage(msgObj, state);
                }).sort(function (msgObjA, msgObjB) {
                    return msgObjA.time - msgObjB.time;
                }); // sort by time

            useMessages.forEach(function (msgObj) {
                if (state.messagesCallback) {
                    state.eMIs.add(msgObj.id);
                    state.messagesCallback(msgObj.data);
                }
            });
            return Promise.resolve();
        });
    }

    function close$2(channelState) {
        channelState.closed = true;
        channelState.db.close();
    }
    function postMessage$2(channelState, messageJson) {
        channelState.writeBlockPromise = channelState.writeBlockPromise.then(function () {
            return writeMessage(channelState.db, channelState.uuid, messageJson);
        }).then(function () {
            if (randomInt(0, 10) === 0) {
                /* await (do not await) */
                cleanOldMessages(channelState.db, channelState.options.idb.ttl);
            }
        });
        return channelState.writeBlockPromise;
    }
    function onMessage$2(channelState, fn, time) {
        channelState.messagesCallbackTime = time;
        channelState.messagesCallback = fn;
        readNewMessages(channelState);
    }
    function canBeUsed$2() {
        if (isNode) return false;
        var idb = getIdb();
        if (!idb) return false;
        return true;
    }
    function averageResponseTime$2(options) {
        return options.idb.fallbackInterval * 2;
    }
    var IndexeDbMethod = {
        create: create$2,
        close: close$2,
        onMessage: onMessage$2,
        postMessage: postMessage$2,
        canBeUsed: canBeUsed$2,
        type: type$2,
        averageResponseTime: averageResponseTime$2,
        microSeconds: microSeconds$2
    };

    /**
     * A localStorage-only method which uses localstorage and its 'storage'-event
     * This does not work inside of webworkers because they have no access to locastorage
     * This is basically implemented to support IE9 or your grandmothers toaster.
     * @link https://caniuse.com/#feat=namevalue-storage
     * @link https://caniuse.com/#feat=indexeddb
     */
    var microSeconds$1 = microSeconds$4;
    var KEY_PREFIX = 'pubkey.broadcastChannel-';
    var type$1 = 'localstorage';
    /**
     * copied from crosstab
     * @link https://github.com/tejacques/crosstab/blob/master/src/crosstab.js#L32
     */

    function getLocalStorage() {
        var localStorage;
        if (typeof window === 'undefined') return null;

        try {
            localStorage = window.localStorage;
            localStorage = window['ie8-eventlistener/storage'] || window.localStorage;
        } catch (e) {// New versions of Firefox throw a Security exception
            // if cookies are disabled. See
            // https://bugzilla.mozilla.org/show_bug.cgi?id=1028153
        }

        return localStorage;
    }
    function storageKey(channelName) {
        return KEY_PREFIX + channelName;
    }
    /**
    * writes the new message to the storage
    * and fires the storage-event so other readers can find it
    */

    function postMessage$1(channelState, messageJson) {
        return new Promise(function (res) {
            sleep().then(function () {
                var key = storageKey(channelState.channelName);
                var writeObj = {
                    token: randomToken(),
                    time: new Date().getTime(),
                    data: messageJson,
                    uuid: channelState.uuid
                };
                var value = JSON.stringify(writeObj);
                getLocalStorage().setItem(key, value);
                /**
                 * StorageEvent does not fire the 'storage' event
                 * in the window that changes the state of the local storage.
                 * So we fire it manually
                 */

                var ev = document.createEvent('Event');
                ev.initEvent('storage', true, true);
                ev.key = key;
                ev.newValue = value;
                window.dispatchEvent(ev);
                res();
            });
        });
    }
    function addStorageEventListener(channelName, fn) {
        var key = storageKey(channelName);

        var listener = function listener(ev) {
            if (ev.key === key) {
                fn(JSON.parse(ev.newValue));
            }
        };

        window.addEventListener('storage', listener);
        return listener;
    }
    function removeStorageEventListener(listener) {
        window.removeEventListener('storage', listener);
    }
    function create$1(channelName, options) {
        options = fillOptionsWithDefaults$1(options);

        if (!canBeUsed$1()) {
            throw new Error('BroadcastChannel: localstorage cannot be used');
        }

        var uuid = randomToken();
        /**
         * eMIs
         * contains all messages that have been emitted before
         * @type {ObliviousSet}
         */

        var eMIs = new ObliviousSet(options.localstorage.removeTimeout);
        var state = {
            channelName: channelName,
            uuid: uuid,
            eMIs: eMIs // emittedMessagesIds

        };
        state.listener = addStorageEventListener(channelName, function (msgObj) {
            if (!state.messagesCallback) return; // no listener

            if (msgObj.uuid === uuid) return; // own message

            if (!msgObj.token || eMIs.has(msgObj.token)) return; // already emitted

            if (msgObj.data.time && msgObj.data.time < state.messagesCallbackTime) return; // too old

            eMIs.add(msgObj.token);
            state.messagesCallback(msgObj.data);
        });
        return state;
    }
    function close$1(channelState) {
        removeStorageEventListener(channelState.listener);
    }
    function onMessage$1(channelState, fn, time) {
        channelState.messagesCallbackTime = time;
        channelState.messagesCallback = fn;
    }
    function canBeUsed$1() {
        if (isNode) return false;
        var ls = getLocalStorage();
        if (!ls) return false;

        try {
            var key = '__broadcastchannel_check';
            ls.setItem(key, 'works');
            ls.removeItem(key);
        } catch (e) {
            // Safari 10 in private mode will not allow write access to local
            // storage and fail with a QuotaExceededError. See
            // https://developer.mozilla.org/en-US/docs/Web/API/Web_Storage_API#Private_Browsing_Incognito_modes
            return false;
        }

        return true;
    }
    function averageResponseTime$1() {
        var defaultTime = 120;
        var userAgent = navigator.userAgent.toLowerCase();

        if (userAgent.includes('safari') && !userAgent.includes('chrome')) {
            // safari is much slower so this time is higher
            return defaultTime * 2;
        }

        return defaultTime;
    }
    var LocalstorageMethod = {
        create: create$1,
        close: close$1,
        onMessage: onMessage$1,
        postMessage: postMessage$1,
        canBeUsed: canBeUsed$1,
        type: type$1,
        averageResponseTime: averageResponseTime$1,
        microSeconds: microSeconds$1
    };

    var microSeconds = microSeconds$4;
    var type = 'simulate';
    var SIMULATE_CHANNELS = new Set();
    function create(channelName) {
        var state = {
            name: channelName,
            messagesCallback: null
        };
        SIMULATE_CHANNELS.add(state);
        return state;
    }
    function close(channelState) {
        SIMULATE_CHANNELS["delete"](channelState);
    }
    function postMessage(channelState, messageJson) {
        return new Promise(function (res) {
            return setTimeout(function () {
                var channelArray = Array.from(SIMULATE_CHANNELS);
                channelArray.filter(function (channel) {
                    return channel.name === channelState.name;
                }).filter(function (channel) {
                    return channel !== channelState;
                }).filter(function (channel) {
                    return !!channel.messagesCallback;
                }).forEach(function (channel) {
                    return channel.messagesCallback(messageJson);
                });
                res();
            }, 5);
        });
    }
    function onMessage(channelState, fn) {
        channelState.messagesCallback = fn;
    }
    function canBeUsed() {
        return true;
    }
    function averageResponseTime() {
        return 5;
    }
    var SimulateMethod = {
        create: create,
        close: close,
        onMessage: onMessage,
        postMessage: postMessage,
        canBeUsed: canBeUsed,
        type: type,
        averageResponseTime: averageResponseTime,
        microSeconds: microSeconds
    };

    var METHODS = [NativeMethod, // fastest
        IndexeDbMethod, LocalstorageMethod];
    /**
     * The NodeMethod is loaded lazy
     * so it will not get bundled in browser-builds
     */

    if (isNode) {
        /**
         * we use the non-transpiled code for nodejs
         * because it runs faster
         */
        var NodeMethod$1 = require('../../src/methods/' + // use this hack so that browserify and others
            // do not import the node-method by default
            // when bundling.
            'node.js');
        /**
         * this will be false for webpackbuilds
         * which will shim the node-method with an empty object {}
         */


        if (typeof NodeMethod$1.canBeUsed === 'function') {
            METHODS.push(NodeMethod$1);
        }
    }

    function chooseMethod(options) {
        var chooseMethods = [].concat(options.methods, METHODS).filter(Boolean); // directly chosen

        if (options.type) {
            if (options.type === 'simulate') {
                // only use simulate-method if directly chosen
                return SimulateMethod;
            }

            var ret = chooseMethods.find(function (m) {
                return m.type === options.type;
            });
            if (!ret) throw new Error('method-type ' + options.type + ' not found'); else return ret;
        }
        /**
         * if no webworker support is needed,
         * remove idb from the list so that localstorage is been chosen
         */


        if (!options.webWorkerSupport && !isNode) {
            chooseMethods = chooseMethods.filter(function (m) {
                return m.type !== 'idb';
            });
        }

        var useMethod = chooseMethods.find(function (method) {
            return method.canBeUsed();
        });
        if (!useMethod) throw new Error('No useable methode found:' + JSON.stringify(METHODS.map(function (m) {
            return m.type;
        }))); else return useMethod;
    }

    var BroadcastChannel$1 = function BroadcastChannel(name, options) {
        this.name = name;

        this.options = fillOptionsWithDefaults$1(options);
        this.method = chooseMethod(this.options); // isListening

        this._iL = false;
        /**
         * _onMessageListener
         * setting onmessage twice,
         * will overwrite the first listener
         */

        this._onML = null;
        /**
         * _addEventListeners
         */

        this._addEL = {
            message: [],
            internal: []
        };
        /**
         * _beforeClose
         * array of promises that will be awaited
         * before the channel is closed
         */

        this._befC = [];
        /**
         * _preparePromise
         */

        this._prepP = null;

        _prepareChannel(this);
    }; // STATICS

    /**
     * used to identify if someone overwrites
     * window.BroadcastChannel with this
     * See methods/native.js
     */

    BroadcastChannel$1._pubkey = true;

    BroadcastChannel$1.prototype = {
        postMessage: function postMessage(msg) {
            if (this.closed) {
                throw new Error('BroadcastChannel.postMessage(): ' + 'Cannot post message after channel has closed');
            }

            return _post(this, 'message', msg);
        },
        postInternal: function postInternal(msg) {
            return _post(this, 'internal', msg);
        },

        set onmessage(fn) {
            var time = this.method.microSeconds();
            var listenObj = {
                time: time,
                fn: fn
            };

            _removeListenerObject(this, 'message', this._onML);

            if (fn && typeof fn === 'function') {
                this._onML = listenObj;

                _addListenerObject(this, 'message', listenObj);
            } else {
                this._onML = null;
            }
        },

        addEventListener: function addEventListener(type, fn) {
            var time = this.method.microSeconds();
            var listenObj = {
                time: time,
                fn: fn
            };

            _addListenerObject(this, type, listenObj);
        },
        removeEventListener: function removeEventListener(type, fn) {
            var obj = this._addEL[type].find(function (obj) {
                return obj.fn === fn;
            });

            _removeListenerObject(this, type, obj);
        },
        close: function close() {
            var _this = this;

            if (this.closed) return;
            this.closed = true;
            var awaitPrepare = this._prepP ? this._prepP : Promise.resolve();
            this._onML = null;
            this._addEL.message = [];
            return awaitPrepare.then(function () {
                return Promise.all(_this._befC.map(function (fn) {
                    return fn();
                }));
            }).then(function () {
                return _this.method.close(_this._state);
            });
        },

        get type() {
            return this.method.type;
        }

    };

    function _post(broadcastChannel, type, msg) {
        var time = broadcastChannel.method.microSeconds();
        var msgObj = {
            time: time,
            type: type,
            data: msg
        };
        var awaitPrepare = broadcastChannel._prepP ? broadcastChannel._prepP : Promise.resolve();
        return awaitPrepare.then(function () {
            return broadcastChannel.method.postMessage(broadcastChannel._state, msgObj);
        });
    }

    function _prepareChannel(channel) {
        var maybePromise = channel.method.create(channel.name, channel.options);

        if (isPromise(maybePromise)) {
            channel._prepP = maybePromise;
            maybePromise.then(function (s) {
                // used in tests to simulate slow runtime

                /*if (channel.options.prepareDelay) {
                     await new Promise(res => setTimeout(res, this.options.prepareDelay));
                }*/
                channel._state = s;
            });
        } else {
            channel._state = maybePromise;
        }
    }

    function _hasMessageListeners(channel) {
        if (channel._addEL.message.length > 0) return true;
        if (channel._addEL.internal.length > 0) return true;
        return false;
    }

    function _addListenerObject(channel, type, obj) {
        channel._addEL[type].push(obj);

        _startListening(channel);
    }

    function _removeListenerObject(channel, type, obj) {
        channel._addEL[type] = channel._addEL[type].filter(function (o) {
            return o !== obj;
        });

        _stopListening(channel);
    }

    function _startListening(channel) {
        if (!channel._iL && _hasMessageListeners(channel)) {
            // someone is listening, start subscribing
            var listenerFn = function listenerFn(msgObj) {
                channel._addEL[msgObj.type].forEach(function (obj) {
                    if (msgObj.time >= obj.time) {
                        obj.fn(msgObj.data);
                    }
                });
            };

            var time = channel.method.microSeconds();

            if (channel._prepP) {
                channel._prepP.then(function () {
                    channel._iL = true;
                    channel.method.onMessage(channel._state, listenerFn, time);
                });
            } else {
                channel._iL = true;
                channel.method.onMessage(channel._state, listenerFn, time);
            }
        }
    }

    function _stopListening(channel) {
        if (channel._iL && !_hasMessageListeners(channel)) {
            // noone is listening, stop subscribing
            channel._iL = false;
            var time = channel.method.microSeconds();
            channel.method.onMessage(channel._state, null, time);
        }
    }

    // Only Node.JS has a process variable that is of [[Class]] process
    var detectNode = Object.prototype.toString.call(typeof process !== 'undefined' ? process : 0) === '[object process]';

    /* global WorkerGlobalScope */
    function add$2(fn) {
        if (typeof WorkerGlobalScope === 'function' && self instanceof WorkerGlobalScope); else {
            /**
             * if we are on react-native, there is no window.addEventListener
             * @link https://github.com/pubkey/unload/issues/6
             */
            if (typeof window.addEventListener !== 'function') return;
            /**
             * for normal browser-windows, we use the beforeunload-event
             */

            window.addEventListener('beforeunload', function () {
                fn();
            }, true);
            /**
             * for iframes, we have to use the unload-event
             * @link https://stackoverflow.com/q/47533670/3443137
             */

            window.addEventListener('unload', function () {
                fn();
            }, true);
        }
        /**
         * TODO add fallback for safari-mobile
         * @link https://stackoverflow.com/a/26193516/3443137
         */

    }

    var BrowserMethod = {
        add: add$2
    };

    // set to true to log events

    function add$1(fn) {
        process.on('exit', function () {
            return fn();
        });
        /**
         * on the following events,
         * the process will not end if there are
         * event-handlers attached,
         * therefore we have to call process.exit()
         */

        process.on('beforeExit', function () {
            return fn().then(function () {
                return process.exit();
            });
        }); // catches ctrl+c event

        process.on('SIGINT', function () {
            return fn().then(function () {
                return process.exit();
            });
        }); // catches uncaught exceptions

        process.on('uncaughtException', function (err) {
            return fn().then(function () {
                console.trace(err);
                process.exit(1);
            });
        });
    }

    var NodeMethod = {
        add: add$1
    };

    var USE_METHOD = detectNode ? NodeMethod : BrowserMethod;
    var LISTENERS = new Set();
    var startedListening = false;

    function startListening() {
        if (startedListening) return;
        startedListening = true;
        USE_METHOD.add(runAll);
    }

    function add(fn) {
        startListening();
        if (typeof fn !== 'function') throw new Error('Listener is no function');
        LISTENERS.add(fn);
        var addReturn = {
            remove: function remove() {
                return LISTENERS["delete"](fn);
            },
            run: function run() {
                LISTENERS["delete"](fn);
                return fn();
            }
        };
        return addReturn;
    }
    function runAll() {
        var promises = [];
        LISTENERS.forEach(function (fn) {
            promises.push(fn());
            LISTENERS["delete"](fn);
        });
        return Promise.all(promises);
    }
    function removeAll() {
        LISTENERS.clear();
    }
    function getSize() {
        return LISTENERS.size;
    }
    var unload = {
        add: add,
        runAll: runAll,
        removeAll: removeAll,
        getSize: getSize
    };

    var LeaderElection = function LeaderElection(channel, options) {
        this._channel = channel;
        this._options = options;
        this.isLeader = false;
        this.isDead = false;
        this.token = randomToken();
        this._isApl = false; // _isApplying

        this._reApply = false; // things to clean up

        this._unl = []; // _unloads

        this._lstns = []; // _listeners

        this._invs = []; // _intervals
    };

    LeaderElection.prototype = {
        applyOnce: function applyOnce() {
            var _this = this;

            if (this.isLeader) return Promise.resolve(false);
            if (this.isDead) return Promise.resolve(false); // do nothing if already running

            if (this._isApl) {
                this._reApply = true;
                return Promise.resolve(false);
            }

            this._isApl = true;
            var stopCriteria = false;

            var handleMessage = function handleMessage(msg) {
                if (msg.context === 'leader' && msg.token != _this.token) {

                    if (msg.action === 'apply') {
                        // other is applying
                        if (msg.token > _this.token) {
                            // other has higher token, stop applying
                            stopCriteria = true;
                        }
                    }

                    if (msg.action === 'tell') {
                        // other is already leader
                        stopCriteria = true;
                    }
                }
            };

            this._channel.addEventListener('internal', handleMessage);

            var ret = _sendMessage(this, 'apply') // send out that this one is applying
                .then(function () {
                    return sleep(_this._options.responseTime);
                }) // let others time to respond
                .then(function () {
                    if (stopCriteria) return Promise.reject(new Error()); else return _sendMessage(_this, 'apply');
                }).then(function () {
                    return sleep(_this._options.responseTime);
                }) // let others time to respond
                .then(function () {
                    if (stopCriteria) return Promise.reject(new Error()); else return _sendMessage(_this);
                }).then(function () {
                    return _beLeader(_this);
                }) // no one disagreed -> this one is now leader
                .then(function () {
                    return true;
                })["catch"](function () {
                    return false;
                }) // apply not successfull
                .then(function (success) {
                    _this._channel.removeEventListener('internal', handleMessage);

                    _this._isApl = false;

                    if (!success && _this._reApply) {
                        _this._reApply = false;
                        return _this.applyOnce();
                    } else return success;
                });

            return ret;
        },
        awaitLeadership: function awaitLeadership() {
            if (
                /* _awaitLeadershipPromise */
                !this._aLP) {
                this._aLP = _awaitLeadershipOnce(this);
            }

            return this._aLP;
        },
        die: function die() {
            var _this2 = this;

            if (this.isDead) return;
            this.isDead = true;

            this._lstns.forEach(function (listener) {
                return _this2._channel.removeEventListener('internal', listener);
            });

            this._invs.forEach(function (interval) {
                return clearInterval(interval);
            });

            this._unl.forEach(function (uFn) {
                uFn.remove();
            });

            return _sendMessage(this, 'death');
        }
    };

    function _awaitLeadershipOnce(leaderElector) {
        if (leaderElector.isLeader) return Promise.resolve();
        return new Promise(function (res) {
            var resolved = false;

            var finish = function finish() {
                if (resolved) return;
                resolved = true;
                clearInterval(interval);

                leaderElector._channel.removeEventListener('internal', whenDeathListener);

                res(true);
            }; // try once now


            leaderElector.applyOnce().then(function () {
                if (leaderElector.isLeader) finish();
            }); // try on fallbackInterval

            var interval = setInterval(function () {
                leaderElector.applyOnce().then(function () {
                    if (leaderElector.isLeader) finish();
                });
            }, leaderElector._options.fallbackInterval);

            leaderElector._invs.push(interval); // try when other leader dies


            var whenDeathListener = function whenDeathListener(msg) {
                if (msg.context === 'leader' && msg.action === 'death') {
                    leaderElector.applyOnce().then(function () {
                        if (leaderElector.isLeader) finish();
                    });
                }
            };

            leaderElector._channel.addEventListener('internal', whenDeathListener);

            leaderElector._lstns.push(whenDeathListener);
        });
    }
    /**
     * sends and internal message over the broadcast-channel
     */


    function _sendMessage(leaderElector, action) {
        var msgJson = {
            context: 'leader',
            action: action,
            token: leaderElector.token
        };
        return leaderElector._channel.postInternal(msgJson);
    }

    function _beLeader(leaderElector) {
        leaderElector.isLeader = true;
        var unloadFn = unload.add(function () {
            return leaderElector.die();
        });

        leaderElector._unl.push(unloadFn);

        var isLeaderListener = function isLeaderListener(msg) {
            if (msg.context === 'leader' && msg.action === 'apply') {
                _sendMessage(leaderElector, 'tell');
            }
        };

        leaderElector._channel.addEventListener('internal', isLeaderListener);

        leaderElector._lstns.push(isLeaderListener);

        return _sendMessage(leaderElector, 'tell');
    }

    function fillOptionsWithDefaults(options, channel) {
        if (!options) options = {};
        options = JSON.parse(JSON.stringify(options));

        if (!options.fallbackInterval) {
            options.fallbackInterval = 3000;
        }

        if (!options.responseTime) {
            options.responseTime = channel.method.averageResponseTime(channel.options);
        }

        return options;
    }

    function createLeaderElection(channel, options) {
        if (channel._leaderElector) {
            throw new Error('BroadcastChannel already has a leader-elector');
        }

        options = fillOptionsWithDefaults(options, channel);
        var elector = new LeaderElection(channel, options);

        channel._befC.push(function () {
            return elector.die();
        });

        channel._leaderElector = elector;
        return elector;
    }

    function f(n) {
        return n < 10 ? "0" + n : n;
    }

    Date.prototype.toJSON = function (key) {
        return isFinite(this.valueOf()) ? this.getFullYear() + '-' + f(this.getMonth() + 1) + '-' + f(this.getDate()) + ' ' + f(this.getHours()) + ':' + f(this.getMinutes()) + ':' + f(this.getSeconds()) : null;
    };

    if (!BroadcastChannel$1.prototype._orginPostMessage) BroadcastChannel$1.prototype._orginPostMessage = BroadcastChannel$1.prototype.postMessage;
    if (!BroadcastChannel$1.prototype._orginClose) BroadcastChannel$1.prototype._orginClose = BroadcastChannel$1.prototype.close;

    BroadcastChannel$1.prototype.postMessage = function (msg) {
        //重写postMessage
        if (this.closed) {
            throw new Error('连接已关闭，无法发送信息。');
        }

        if (this.onSendMessage) this.onSendMessage(msg);
        return this._orginPostMessage(msg);
    };

    BroadcastChannel$1.prototype.close = function () {
        //重写关闭事件
        if (this.onBeforeClose) this.onBeforeClose();

        this._orginClose();

        if (this.onAfterClose) this.onAfterClose();
    };

    BroadcastChannel$1.getInstance = function (options) {
        //获取广播和signalR实例
        var error = function error(msg) {
            if (console && console.log) console.log(msg);
        };

        var isToSignalR = function isToSignalR(msg) {
            //判断是否发送signalR
            var action = msg.Action || "";
            if (action.indexOf("/") < 0) return false;
            return true;
        };

        var channel = new BroadcastChannel$1("GKSYB", options);

        channel.messageHandler = function (msg) {
            //统一消息处理
            if (!msg.Action) return;
            var action = "on" + msg.Action;

            if (this[action]) {
                this[action](msg.Data, msg.Type);
            }

            if (isToSignalR(msg)) this._postMessageToSignalR(msg);
        };

        channel._signalRMessage = [];
        channel._maxSignalOfflineMessage = 100;

        channel.pushSignalOfflineMessage = function (msg) {
            //加入离线处理
            if (this._signalRMessage.length > this._maxSignalOfflineMessage) this._signalRMessage.shift();

            this._signalRMessage.push(msg);
        };

        channel._postMessageToSignalR = function (msg, method) {
            //发送到signalR
            if (!this._leaderElector.isLeader) return;
            if (!this.connection) return;
            method = method || "Excute";
            msg._method = method;

            if (this.connection.connectionState !== "Connected") {
                //加入缓冲队列
                channel.pushSignalOfflineMessage(msg);
                return;
            }

            this.connection.invoke(msg._method, msg).catch(function (msg) {
                error(msg);
            });
        };

        channel.onSendMessage = function (msg) {
            //本实例发送处理
            if (isToSignalR(msg)) this._postMessageToSignalR(msg);
        };

        var fn = channel.messageHandler.bind(channel);
        channel.removeEventListener('message', fn);
        channel.addEventListener("message", fn);

        channel.onBeforeClose = function () {
            //关闭前先关闭signalR
            if (this.connection) {
                this.connection.stop();
                delete this.connection;
            }
        };

        var leaderElector = BroadcastChannel$1.createLeaderElection(channel); //领导者模式

        leaderElector.awaitLeadership().then(function () {
            //领导者连接signalR
            if (channel.onLeader) channel.onLeader();
            var connection = new signalR.HubConnectionBuilder().withUrl(window.gksybConfigs.apiBase + "broadcast-channel", {
                accessTokenFactory: function accessTokenFactory() {
                    return window.session.Token;
                }
            }).withAutomaticReconnect([0, 0, 3]).build();
            if (!connection._orginStop) connection._orginStop = connection.stop;

            connection.stop = function () {
                //重载关闭
                this._canStop = true;

                this._orginStop();
            };

            connection.channel = channel;
            channel.connection = connection;
            connection.on("Excute", function (msg) {
                //服务器发送消息处理并广播
                msg.Type = "SignalR";
                this.channel.messageHandler(msg);
                this.channel.postMessage(msg);
            });

            var start = function start() {
                //开启signalR
                connection.start().then(function () { }).catch(function (msg) {
                    error(msg);
                });
            };

            connection.onclose(function (error) {
                //关闭时重新连接
                if (this._canStop) return;

                var inner = function inner() {
                    $.ajax({
                        type: 'get',
                        cache: false,
                        url: 'Auth/IsLogin',
                        success: function success(result) {
                            start();
                        },
                        error: function error() {
                            setTimeout(inner, 3000);
                        }
                    });
                };

                inner();
            });
            if (channel.onSignalR) channel.onSignalR(connect); //signalR重新拦截

            var sendOfflineMessage = function sendOfflineMessage() {
                //定时发送离线消息
                try {
                    if (channel.connection.connectionState === "Connected") {
                        //加入缓冲队列
                        var msg = channel._signalRMessage.shift();

                        if (msg) {
                            this.connection.invoke(msg._method, msg).catch(function (msg) {
                                error(msg);
                            });
                        }
                    }
                } catch (e) { }
                setTimeout(sendOfflineMessage, 200);
            };

            start();
            sendOfflineMessage();
        });
        return channel;
    };

    BroadcastChannel$1.createLeaderElection = createLeaderElection;
    window['Broadcast'] = BroadcastChannel$1;

})));
