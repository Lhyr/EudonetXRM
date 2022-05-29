/// <summary>
/// 
/// </summary>
var eDictionary = (function () {
    function eDictionary() {
        if (!(this instanceof eDictionary))
            return new eDictionary();
    }

    eDictionary.prototype.Count = function () {
        var key,
            count = 0;

        for (key in this) {
            if (this.hasOwnProperty(key))
                count += 1;
        }
        return count;
    };

    eDictionary.prototype.Keys = function () {
        var key,
            keys = [];

        for (key in this) {
            if (this.hasOwnProperty(key))
                keys.push(key);
        }
        return keys;
    };

    eDictionary.prototype.Values = function () {
        var key,
            values = [];

        for (key in this) {
            if (this.hasOwnProperty(key))
                values.push(this[key]);
        }
        return values;
    };

    eDictionary.prototype.KeyValuePairs = function () {
        var key,
            keyValuePairs = [];

        for (key in this) {
            if (this.hasOwnProperty(key))
                keyValuePairs.push({
                    Key: key,
                    Value: this[key]
                });
        }
        return keyValuePairs;
    };

    eDictionary.prototype.Add = function (key, value) {
        this[key] = value;
    }

    eDictionary.prototype.Clear = function () {
        var key,
            dummy;

        for (key in this) {
            if (this.hasOwnProperty(key))
                dummy = delete this[key];
        }
    }

    eDictionary.prototype.ContainsKey = function (key) {
        return this.hasOwnProperty(key);
    }

    eDictionary.prototype.ContainsValue = function (value) {
        var key;

        for (key in this) {
            if (this.hasOwnProperty(key) && this[key] === value)
                return true;
        }
        return false;
    }

    eDictionary.prototype.Remove = function (key) {
        var dummy;

        if (this.hasOwnProperty(key)) {
            dummy = delete this[key];
            return true;
        } else
            return false;
    }

    return eDictionary;
}());