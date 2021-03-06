// Filter n'est pas supporté par IE8
// Firefox - https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/filter
if (!Array.prototype.filter) {
    Array.prototype.filter = function (fun/*, thisArg*/) {
        'use strict';

        if (this === void 0 || this === null) {
            throw new TypeError();
        }

        var t = Object(this);
        var len = t.length >>> 0;
        if (typeof fun !== 'function') {
            throw new TypeError();
        }

        var res = [];
        var thisArg = arguments.length >= 2 ? arguments[1] : void 0;
        for (var i = 0; i < len; i++) {
            if (i in t) {
                var val = t[i];

                // NOTE: Technically this should Object.defineProperty at
                //       the next index, as push can be affected by
                //       properties on Object.prototype and Array.prototype.
                //       But that method's new, and collisions should be
                //       rare, so use the more-compatible alternative.
                if (fun.call(thisArg, val, i, t)) {
                    res.push(val);
                }
            }
        }

        return res;
    };
}

// Production steps of ECMA-262, Edition 5, 15.4.4.18
// Reference: http://es5.github.io/#x15.4.4.18
if (!Array.prototype.forEach) {

    Array.prototype.forEach = function (callback, thisArg) {

        var T, k;

        if (this === null) {
            throw new TypeError('this is null or not defined');
        }

        // 1. Let O be the result of calling toObject() passing the
        // |this| value as the argument.
        var O = Object(this);

        // 2. Let lenValue be the result of calling the Get() internal
        // method of O with the argument "length".
        // 3. Let len be toUint32(lenValue).
        var len = O.length >>> 0;

        // 4. If isCallable(callback) is false, throw a TypeError exception. 
        // See: http://es5.github.com/#x9.11
        if (typeof callback !== 'function') {
            throw new TypeError(callback + ' is not a function');
        }

        // 5. If thisArg was supplied, let T be thisArg; else let
        // T be undefined.
        if (arguments.length > 1) {
            T = thisArg;
        }

        // 6. Let k be 0
        k = 0;

        // 7. Repeat, while k < len
        while (k < len) {

            var kValue;

            // a. Let Pk be ToString(k).
            //    This is implicit for LHS operands of the in operator
            // b. Let kPresent be the result of calling the HasProperty
            //    internal method of O with argument Pk.
            //    This step can be combined with c
            // c. If kPresent is true, then
            if (k in O) {

                // i. Let kValue be the result of calling the Get internal
                // method of O with argument Pk.
                kValue = O[k];

                // ii. Call the Call internal method of callback with T as
                // the this value and argument list containing kValue, k, and O.
                callback.call(T, kValue, k, O);
            }
            // d. Increase k by 1.
            k++;
        }
        // 8. return undefined
    };
}


/// <summary>Fonction qui permet de sélectionner le parent a partir de l'element enfant</summary>
var fromSource = function (element) {
    // copie interne de l'element source pour une sécurité
    var _element = element;
    var _attributeName = "";
    var _attributeCondition = function (attributeValue) { return false; };
    var _selector = "";

    function attrConditionChecked(element) {

        var attributeValue = _attributeName.toLowerCase() == "id" ? _element.id : getAttributeValue(element, _attributeName);
        if (attributeValue == "")
            return false;

        if (typeof (_attributeCondition) == 'function')
            return _attributeCondition(attributeValue);

        return attributeValue == _attributeCondition;
    }

    return {
        /// Recoit un selector 
        'withSelector': function (selector) {
            _selector = selector + "";
            return this;
        },

        /// Recoit un nom d'attribut 
        'whereAttribute': function (attrName, attrCondition) {

            _attributeName = attrName;
            _attributeCondition = attrCondition;
            return {
                ///  Recherche le parent direct de l'element enfant avec respectant la valeur de l'attribut s'il est fourni ou prend l'id par defaut
                'findParent': function (action) {
                    while (_element != null ||(_element && _element.tagName != 'BODY')) {
                        if (attrConditionChecked(_element)) {
                            action(_element);
                            break;
                        }
                        _element = _element.parentElement;
                    }
                },
            };
        },

        ///  Recherche l'enfant direct et si on trouve on excute l'action
        'selectChild': function (action) {
            if (_element != null) {
                var child = _element.querySelector(_selector);
                if (child != null)
                    action(child);
            }
        },

        ///  Recherche l'enfant direct et si on trouve on excute l'action
        'findChild': function (childId, action) {
            _selector = "#" + childId;
            this.selectChild(action);
        },

        'findParentById': function (parentId, action) {

            while (_element != null || (_element && _element.tagName != 'BODY')) {
                if (_element.id == parentId) {
                    action(_element);
                    break;
                }
                _element = _element.parentElement;
            }
        }
    }
};
