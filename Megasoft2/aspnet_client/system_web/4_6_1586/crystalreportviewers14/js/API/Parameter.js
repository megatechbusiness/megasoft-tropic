SAP.common.defineNS("SAP.CR.Parameter", function(paramName, paramType) {
    if (!(this instanceof SAP.CR.Parameter))
        throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InstantiationFailed.replace("{0}",
                "SAP.CR.Parameter"));

    var EXPECTED_NUM_ARGUMENTS = 2; /* Update when changing constructor*/
    
    if(arguments.length != EXPECTED_NUM_ARGUMENTS) {
        throw SAP.CR.Viewer.Exceptions.MissingArgumentException.create(L_bobj_crv_API_InvalidNumOfArguments.replace("{0}", "SAP.CR.Parameter").replace("{1}", EXPECTED_NUM_ARGUMENTS));
    }
    else if(!bobj.isString(paramName) || paramName.length == 0) {
        throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidParamName);
    }
    else if(!isValidParamType(paramType)) {
        throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidParamType.replace("{0}", paramName));
    }
    
    var name = paramName;
    var reportName = null;
    var values = [];
    var type = paramType;

    
    function isValidParamType(type) {
        var Types = bobj.crv.params.DataTypes;
        for(var name in Types)
            if(Types[name] == type)
                return true;
        
        return false;
    }
    
    /**
     * Returns true if value is valid, otherwise throws exception
     */
    function isValidValue(value) {
        if (value != null) {
            if (value instanceof SAP.CR.Parameter.RangeValue) {
                return value.validate(type, arguments.callee);
            } else {
                var DTs = SAP.CR.Parameter.DataTypes;
                switch (type) {
                case DTs.TIME:
                case DTs.DATE_TIME:
                case DTs.DATE:
                    if (!(value instanceof Date))
                        throw SAP.CR.Viewer.Exceptions.InvalidValueType.create(L_bobj_crv_API_InvalidValueType.replace("{0}",
                                "JavaScript Date Object"));
                    break;
                case DTs.STRING:
                    if (!bobj.isString(value))
                        throw SAP.CR.Viewer.Exceptions.InvalidValueType.create(L_bobj_crv_API_InvalidValueType.replace("{0}", "String"));
                    break;
                case DTs.NUMBER:
                case DTs.CURRENCY:
                    if (!bobj.isNumber(value))
                        throw SAP.CR.Viewer.Exceptions.InvalidValueType.create(L_bobj_crv_API_InvalidValueType.replace("{0}", "Number"));
                    break;
                case DTs.BOOLEAN:
                    if (!bobj.isBoolean(value))
                        throw SAP.CR.Viewer.Exceptions.InvalidValueType.create(L_bobj_crv_API_InvalidValueType.replace("{0}", "Boolean"));
                    break;
                default:
                    throw SAP.CR.Viewer.Exceptions.InvalidValueType.create(L_bobj_crv_API_ValueTypeUndefined);
                }
            }
        }

        return true;
    }

    this.addValue = function(value) {
        try {
            if (isValidValue(value))
                values.push(value);
        } catch (e) {
            throw SAP.CR.Viewer.Exceptions.InvalidParamValueException.create(L_bobj_crv_API_InvalidParamValue.replace("{0}", name), e);
        }

    };

    this.getValues = function() {
        return values;
    };
    
    this.clearValues = function() {
        values.clear();
    };
    
    this.setReportName = function(name) {
        if(name == null || ( bobj.isString(name) && name.length > 0))
            reportName = name;
    };
    
    this.getReportName = function() {
        return reportName;
    };

    /**
     * Private function, return json representation of value
     */
    function getJSONValue(value) {
        if (value instanceof SAP.CR.Parameter.RangeValue) {
            return value.toJSON(arguments.callee);
        } else if (value == null || value === undefined)
            return null;

        var DTs = SAP.CR.Parameter.DataTypes;
        switch (type) {
        case DTs.TIME:
        case DTs.DATE_TIME:
        case DTs.DATE:
            return bobj.crv.params.dateToJson(value);
            break;
        default:
            return value;
        }
    }

    this.json = function() {
        return {
            paramName : (reportName != null) ? reportName + name : name,
            value : MochiKit.Base.map(getJSONValue, values),
            valueDataType : type,
            reportName : reportName 
        };
    };
});

/**
 * Creates a new instance of SAP.CR.Parameter.RangeValue
 * 
 * @param lowerBoundV
 *            [SAP.CR.Parameter.RangeBoundTypes] lower bound type
 * @param beginValueV
 *            [Object] begin value of range that respects parameter data type
 * @param upperBoundV[SAP.CR.Parameter.RangeBoundTypes]
 *            upper bound type
 * @param endValueV
 *            [Object] begin value of range that respects parameter data type
 */
SAP.common.defineNS("SAP.CR.Parameter.RangeValue", function(lowerBoundV, beginValueV, upperBoundV, endValueV) {
    if (!(this instanceof SAP.CR.Parameter.RangeValue))
        throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InstantiationFailed.replace("{0}",
                "SAP.CR.Parameter.RangeValue"));

    var lowerBound = SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED;
    var upperBound = SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED;
    var beginValue = beginValueV;
    var endValue = endValueV;

    function isValidBound(bound) {
        var isValid = false;
        for ( var type in SAP.CR.Parameter.RangeBoundTypes)
            if (SAP.CR.Parameter.RangeBoundTypes[type] == bound)
                return true;

        return false;
    }

    this.setLowerBound = function(lower) {
        if (isValidBound(lower))
            lowerBound = lower;
        else
            throw SAP.CR.Viewer.Exceptions.InvalidRangeBound.create(L_bobj_crv_API_InvalidRangeBound);
    };

    this.getLowerBound = function() {
        return lowerBound;
    };

    this.setUpperBound = function(upper) {
        if (isValidBound(upper))
            upperBound = upper;
        else
            throw SAP.CR.Viewer.Exceptions.InvalidRangeBound.create(L_bobj_crv_API_InvalidRangeBound);
    };

    this.getUpperBound = function() {
        return upperBound;
    };

    this.setBeginValue = function(value) {
        beginValue = value;
    };

    this.getBeginValue = function() {
        return beginValue;
    };

    this.setEndValue = function(end) {
        endValue = end;
    };

    this.getEndValue = function() {
        return endValue;
    };

    this.toJSON = function(jsonValueGen) {
        return {
            "lowerBoundType" : this.getLowerBound(),
            "upperBoundType" : this.getUpperBound(),
            "beginValue" : jsonValueGen(this.getBeginValue()),
            "endValue" : jsonValueGen(this.getEndValue())
        };
    }

    function normalizeValue(value, type) {
        var DTs = SAP.CR.Parameter.DataTypes;
        switch (type) {
        case DTs.TIME:
            value.setFullYear(2010);
            value.setMonth(0);
            value.setDate(1);
            break;
        case DTs.DATE:
            value.setHours(0);
            value.setMinutes(0);
            value.setSeconds(0);
            value.setMilliseconds(0);
            break;
        }
    }

    this.validate = function(type, validateDiscreteValue) {
        try {
            validateDiscreteValue(beginValue);
        } catch (e) {
            throw SAP.CR.Viewer.Exceptions.InvalidRangeValue.create(L_bobj_crv_API_InvalidBeginValue, e);
        }
        try {
            validateDiscreteValue(endValue);
        } catch (e) {
            throw SAP.CR.Viewer.Exceptions.InvalidRangeValue.create(L_bobj_crv_API_InvalidEndValue, e);
        }

        if (beginValue)
            normalizeValue(beginValue, type);

        if (endValue)
            normalizeValue(endValue, type);

        if (upperBound !== SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED && (endValue == null || endValue == undefined))
            throw SAP.CR.Viewer.Exceptions.InvalidValue.create(L_bobj_crv_API_InvalidEndValue);

        if (lowerBound !== SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED && (beginValue == null || beginValue == undefined))
            throw SAP.CR.Viewer.Exceptions.InvalidValue.create(L_bobj_crv_API_InvalidBeginValue);

        if (lowerBound == SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED && upperBound == SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED)
            throw SAP.CR.Viewer.Exceptions.InvalidRangeValue.create(L_bobj_crv_API_InvalidLowerAndUpperRangeBound);

        var DTs = SAP.CR.Parameter.DataTypes;

        if (lowerBound !== SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED && upperBound !== SAP.CR.Parameter.RangeBoundTypes.UNBOUNDED) {
            var isThrowE = false;
            switch (type) {
            case DTs.TIME:
            case DTs.DATE_TIME:
            case DTs.DATE:
                if (beginValue.getTime() > endValue.getTime())
                    isThrowE = true;
                break;
            case DTs.STRING:
            case DTs.NUMBER:
            case DTs.CURRENCY:
                if (beginValue > endValue)
                    isThrowE = true;
                break;
            }

            if (isThrowE)
                throw SAP.CR.Viewer.Exceptions.InvalidRangeValue.create(L_bobj_crv_API_RangeBeginValueGreaterThanEndValue);

        }

        if ((lowerBound == SAP.CR.Parameter.RangeBoundTypes.INCLUSIVE && upperBound == SAP.CR.Parameter.RangeBoundTypes.EXCLUSIVE)
                || (lowerBound == SAP.CR.Parameter.RangeBoundTypes.EXCLUSIVE && upperBound == SAP.CR.Parameter.RangeBoundTypes.INCLUSIVE)) {
            var isThrowE = false;
            switch (type) {
            case DTs.TIME:
            case DTs.DATE_TIME:
            case DTs.DATE:
                if (beginValue.getTime() == endValue.getTime())
                    isThrowE = true;
                break;
            case DTs.STRING:
            case DTs.NUMBER:
            case DTs.CURRENCY:
                if (beginValue == endValue)
                    isThrowE = true;
                break;
            }

            if (isThrowE)
                throw SAP.CR.Viewer.Exceptions.InvalidRangeValue.create(L_bobj_crv_API_InvalidBeginAndEndValue);
        }
        
        return true; /* if it gets here, it means the value is valid */
    };

    if (lowerBoundV)
        this.setLowerBound(lowerBoundV);

    if (upperBoundV)
        this.setUpperBound(upperBoundV);

});

SAP.common.defineNS("SAP.CR.Parameter.DataTypes", bobj.crv.params.DataTypes);
SAP.common.defineNS("SAP.CR.Parameter.RangeBoundTypes", bobj.crv.params.RangeBoundTypes);
