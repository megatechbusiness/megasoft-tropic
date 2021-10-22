bobj.crv.ColorPalette = function(color) {
    var hsvColor = RGB2HSV(hex2RGB(color));
    
    /**
     * Sets the saturation and brightness of the given color
     * @param color [string] hex representation of the color to set (ie. #ffffff)
     * @param s [int] saturation value between 0 and 100
     * @param v [int] brightness value between 0 and 100
     * @return RGB value of the modified color
     */
    this.getSpecifiedColor = function(s, v) {
        var newHSV = new HSV(hsvColor.h, s, v);
        newHSV.validate();
        
        return RGB2hex(HSV2RGB(newHSV));
    };
    
    /**
     * Modifies the given color's HSV values by the specified values
     * @param color [string] hex representation of the color to set (ie. #ffffff)
     * @param hDelta [int] the amount to change the hue value
     * @param sRatio [int] the amount to change the saturation value
     * @param vRatio [int] the amount to change the brightness value
     * @return hex representation of the modified color
     */
    this.getModifiedColor = function(hDelta, sRatio, vRatio) {
        var newHSV = new HSV(0, 0, 0);
        
        newHSV.h = (hsvColor.h + hDelta) % 360;
        if (sRatio > 0) newHSV.s = hsvColor.s + ((100 - hsvColor.s) * sRatio);
        else newHSV.s = hsvColor.s + (hsvColor.s * sRatio);
        if (vRatio > 0) newHSV.v = hsvColor.v + ((100 - hsvColor.v) * vRatio);
        else newHSV.v = hsvColor.v + (hsvColor.v * vRatio);
        
        newHSV.validate();
        
        return RGB2hex(HSV2RGB(newHSV));
    };

    function HSV (hue, saturation, value) {
        this.h = hue;
        this.s = saturation;
        this.v = value;
        
        this.validate = function () {
            if (this.h <= 0) {this.h = 0;}
            if (this.s <= 0) {this.s = 0;}
            if (this.v <= 0) {this.v = 0;}
            if (this.h > 360) {this.h = 360;}
            if (this.s > 100) {this.s = 100;}
            if (this.v > 100) {this.v = 100;}
        };
    };

    function RGB (red, green, blue) {
        this.r = red;
        this.g = green;
        this.b = blue;
        
        this.validate = function () {
            if (this.r <= 0) {this.r = 0;}
            if (this.g <= 0) {this.g = 0;}
            if (this.b <= 0) {this.b = 0;}
            if (this.r > 255) {this.r = 255;}
            if (this.g > 255) {this.g = 255;}
            if (this.b > 255) {this.b = 255;}
        };
    };
    
    function hexify (number) {
        var digits = '0123456789ABCDEF';
        var lsd = number % 16;
        var msd = (number - lsd) / 16;
        var hexified = digits.charAt(msd) + digits.charAt(lsd);
        return hexified;
    };

    function decimalize (hexNumber) {
        var digits = '0123456789ABCDEF';
        return ((digits.indexOf(hexNumber.charAt(0).toUpperCase()) * 16) + digits.indexOf(hexNumber.charAt(1).toUpperCase()));
    };
    
    function hex2RGB (colorString) {
        var r = decimalize(colorString.substring(1,3));
        var g = decimalize(colorString.substring(3,5));
        var b = decimalize(colorString.substring(5,7));
        
        return new RGB(r, g, b);
    };

    function RGB2hex (rgb) {
        return "#" + hexify(rgb.r) + hexify(rgb.g) + hexify(rgb.b);
    };
    
    function RGB2HSV (rgb) {
        var hsv = new HSV(0, 0, 0);
        
        var r = rgb.r / 255; 
        var g = rgb.g / 255; 
        var b = rgb.b / 255; // Scale to unity.

        var minVal = Math.min(r, g, b);
        var maxVal = Math.max(r, g, b);
        var delta = maxVal - minVal;

        hsv.v = maxVal;

        if (delta == 0) {
            hsv.h = 0;
            hsv.s = 0;
        } else {
            hsv.s = delta / maxVal;
            var del_R = (((maxVal - r) / 6) + (delta / 2)) / delta;
            var del_G = (((maxVal - g) / 6) + (delta / 2)) / delta;
            var del_B = (((maxVal - b) / 6) + (delta / 2)) / delta;

            if (r == maxVal) {hsv.h = del_B - del_G;}
            else if (g == maxVal) {hsv.h = (1 / 3) + del_R - del_B;}
            else if (b == maxVal) {hsv.h = (2 / 3) + del_G - del_R;}
            
            if (hsv.h < 0) {hsv.h += 1;}
            if (hsv.h > 1) {hsv.h -= 1;}
        }
        hsv.h *= 360;
        hsv.s *= 100;
        hsv.v *= 100;
        
        return hsv;
    };

    function HSV2RGB (hsv) {
        var rgb = new RGB (0, 0, 0);
        
        var h = hsv.h / 360; var s = hsv.s / 100; var v = hsv.v / 100;
        if (s == 0) {
            rgb.r = v * 255;
            rgb.g = v * 255;
            rgb.b = v * 255;
        } else {
            var var_h = h * 6;
            var var_i = Math.floor(var_h);
            var var_1 = v * (1 - s);
            var var_2 = v * (1 - s * (var_h - var_i));
            var var_3 = v * (1 - s * (1 - (var_h - var_i)));
            
            var var_r = v;
            var var_g = var_3;
            var var_b = var_1;
            
            if (var_i == 0) {var_r = v; var_g = var_3; var_b = var_1;}
            else if (var_i == 1) {var_r = var_2; var_g = v; var_b = var_1;}
            else if (var_i == 2) {var_r = var_1; var_g = v; var_b = var_3;}
            else if (var_i == 3) {var_r = var_1; var_g = var_2; var_b = v;}
            else if (var_i == 4) {var_r = var_3; var_g = var_1; var_b = v;}
            else {var_r = v; var_g = var_1; var_b = var_2;}
            
            rgb.r = var_r * 255;
            rgb.g = var_g * 255;
            rgb.b = var_b * 255;
        }
        
        return rgb;
    };
};