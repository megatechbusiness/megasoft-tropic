/* Copyright (c) Business Objects 2006. All rights reserved. */

if (typeof(bobj.html) == 'undefined') {
    bobj.html = {};    
}

bobj.html.openTag = function(tag, atts) {
    var html = '<' + tag;
    
    for (var i in atts) {
        
        html += ' ' + i + '="';
        
        var value = atts[i];
        
        if (bobj.isArray(value)) {
            value = value.join(' ');
        }
        else if (bobj.isObject(value)) {
            // create a string of the form "foo:bar;baz:garply"
            var stringValue = ""
            for (var k in value) {
                stringValue += k + ':' + value[k] + ';';
            }
            value = stringValue;
        }
            
        html += value + '"';
    }
    
    return html + '>' 
};

bobj.html.closeTag = function(tag) {
    return '</' + tag + '>';
};

bobj.html.createHtml = function(tag, atts, innerHtml /*, innerHtml...*/) {
    var html = bobj.html.openTag(tag, atts);
    
    for (var i = 2; i < arguments.length; ++i) {
        html += arguments[i];
    }
    
    html += bobj.html.closeTag(tag);
    return html;
};


MochiKit.Iter.forEach( [ 'table', 'ul', 'ol', 'li', 'td', 'tr', 'tbody', 'thead', 'tfoot', 'th', 'input', 'span', 'a', 'div', 'img',
        'button', 'tt', 'pre', 'h1', 'h2', 'h3', 'br', 'label', 'textarea', 'form', 'p', 'select', 'option', 'optgroup', 'legend',
        'fieldset', 'strong', 'canvas', 'iframe', 'script', 'b' ], function(tagName) {
    bobj.html[tagName.toUpperCase()] = MochiKit.Base.partial(bobj.html.createHtml, tagName)
});



/**
 * Extract scripts from an html fragment.
 * 
 * @param html
 *            [String] html fragment
 * 
 * @return [Object] Returns an object with a list of scripts and html without
 *         scripts i.e. {scripts: [{src:[String], text:[String]},...],
 *         html:[String]}
 */
bobj.html.extractScripts = function(html) {
    // Match script tags with or without closing tags
    // 1 - Attributes of tag that doesn't have closing tag (e.g. <script/>)
    // 2 - Attributes of tag that has closing tag
    // 3 - Script text
    //                            1                 2       3
    var regexpScript = /(?:<script([^>]*)\/>|<script([^>]*)>([\s\S]*?)<\/script>)/i;
    
    // Match src attributes
    // 1 - URI 
    //                    1      
    var regexpSrc = /src=\"([^\"]*)\"/i;
    
    var scripts = [];
    var match = null;
    
    while(match = regexpScript.exec(html)) {
        var script = {src: null, text: null};
        
        var attributes = match[1] || match[2];
        if (attributes = regexpSrc.exec(attributes)) {
            script.src = attributes[1];
        }
        
        if (match[3]) { 
            script.text = match[3];
        }
        
        scripts.push(script);
        html = bobj.extractRange(html, match.index, match.index + match[0].length);
    }
    
    return {scripts: scripts, html: html};
}

/**
 * Extract scripts and css links from a html fragment.
 *
 * @param html [String] html fragment
 *
 * @return [Object] Returns an object with a list of scripts, css links and html
 * i.e. {scripts: [{src:[String], text:[String]},...], html:[String], links: [String,...]}
 */
bobj.html.extractHtml = function(html) {
    var extScripts = bobj.html.extractScripts(html);
    var extLinks = bobj.html.extractLinks(extScripts.html);
    var extStyles = bobj.html.extractStyles(extLinks.html);
    
    return {scripts: extScripts.scripts, html : extStyles.html, links : extLinks.links, styles: extStyles.styles};
}

/**
 * Extract css links from a html fragment.
 *
 * @param html [String] html fragment
 *
 * @return [Object] Returns an object with a list of links and html without
 *                  links 
 * i.e. {links: [String,...], html:[String]}
 */
bobj.html.extractLinks = function(html) {
    var regexpLink = /<link([^>]*)>/i;
    var regexpHref = /href=\"([^\"]*)\"/i;
    
    var links = [];
    var match = null;
    
    while(match = regexpLink.exec(html)) {
        // match = "<link href='' \> 
        
        var href = regexpHref.exec(match);
        
        if(href && href.length > 0) {
            links.push(href[1]);
        }
        html = bobj.extractRange(html, match.index, match.index + match[0].length);
    }

    return {links: links, html: html};
}

/**
 * Extract <style> tags from an html fragment.
 * Note - links to external style sheets are not currently extracted
 *
 * @param html [String] html fragment
 *
 * @return [Object] Returns an object with a list of styles and html without
 *                  styles
 * i.e. {styles: [{text:[String], type:[String], media:[String]},...], html:[String]}
 */
bobj.html.extractStyles = function(html) {
    // Match style tags 
    // 1 - Attributes
    // 2 - CSS text
    //                       1       2                
    var regexpStyle = /<style([^>]*)>([\s\S]*?)<\/style>/i;
    
    var regexpType = /type=\"([^\"]*)\"/i;
    var regexpMedia = /media=\"([^\"]*)\"/i;
    
    var styles = [];
    var match = null;

    while(match = regexpStyle.exec(html)) {
        var style = {media: null, type: null, text: match[2]};
        
        var matchType = regexpType.exec(match[1]);
        if (matchType) {
            style.type = matchType[1];  
        }
        
        var matchMedia = regexpMedia.exec(match[1]); 
        if (matchMedia) {
            style.media = matchMedia[1];  
        }
        
        styles.push(style);
        html = bobj.extractRange(html, match.index, match.index + match[0].length);
    }
    
    return {styles: styles, html: html};
}