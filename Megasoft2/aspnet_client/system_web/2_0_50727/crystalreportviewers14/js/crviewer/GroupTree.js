/*
 ================================================================================
 GroupTree
 ================================================================================
 */

/**
 * GroupTree constructor. 
 *
 * @param kwArgs.id       [String]  DOM node id
 * @param kwArgs.icns     [String]  URL to magnifying glass icon
 * @param kwArgs.minIcon  [String]  URL to min.gif
 * @param kwArgs.plusIcon [String]  URL to plus.gif
 */
bobj.crv.newGroupTree = function(kwArgs) {
    var UPDATE = MochiKit.Base.update;
    kwArgs = UPDATE ( {
        id : bobj.uniqueId (),
        visualStyle : {
            className : null,
            backgroundColor : null,
            borderWidth : null,
            borderStyle : null,
            borderColor : null,
            fontFamily : null,
            fontWeight : null,
            textDecoration : null,
            color : null,
            width : null,
            height : null,
            fontStyle : null,
            fontSize : null
        },
        icns : bobj.crvUri ('images/magnify' + (bobj.crv.config.isRTL ? '_rtl' : '')+ '.gif'),
        minIcon : bobj.crvUri ('images/min.gif'),
        plusIcon : bobj.crvUri ('images/plus.gif')
    }, kwArgs);

    var o = newTreeWidget (kwArgs.id + '_tree', '100%', '100%', kwArgs.icns, null, null, 'groupTree',
            bobj.crv.GroupTree._expand, bobj.crv.GroupTree._collapse, null, kwArgs.minIcon, kwArgs.plusIcon);

    o.vsbar = new bobj.crv.VerticalScrollBar();
    o.hsbar = new bobj.crv.HorizontalScrollBar();
    
    o.SCROLLBAR_SIZE = 16; // width/height of the scrollbars and the arrow buttons
    
    o._children = [];
    o._modalChildren = [];
    o._lastNodeIdInitialized = -1;
    o._lastNodeInitialized = null;
    o._curSigs = [];
    bobj.fillIn (o, kwArgs);
    o.widgetType = 'GroupTree';
    o.initOld = o.init;

    UPDATE (o, bobj.crv.GroupTree);
   
    return o;
};

bobj.crv.GroupTree = {
    /**
     * Disposes group tree
     */
    dispose : function() {
        /* removes all the signals */
        while (this._curSigs.length > 0) {
            bobj.crv.SignalDisposer.dispose (this._curSigs.pop ());
        }
         
        /* disposes all the children*/
        while (this._children.length > 0) {
            var child = this._children.pop ();
            child.dispose ();
            bobj.deleteWidget (child);
            delete child;
        }

        this._lastNodeIdInitialized = -1;
        this._lastNodeInitialized = null;
        this.sub = [];
        bobj.removeAllChildElements(this.treeLyr);
    },
    
    beginHTML : function () {
        return '<label class="crvHidden" id="' + this.id + '_label">' + L_bobj_crv_GroupTree + '</label>' + 
               '<div tabindex=-1  align= "' + (bobj.crv.config.isRTL?"right":"left") + '" id="'+this.id+'" style="position:relative;">'+
               '<div aria-labelledby="' + this.id + '_label" id="'+this.id+'_ctr" style="position:absolute;overflow:hidden;padding:'+this.padding+'px">';
    },
    
    setSelected : function(isSelected) {
        if(isSelected)
            this.getCtrNode().setAttribute("role", "navigation");
        else
            this.getCtrNode().removeAttribute("role");
    },
    
    focusFirstChild : function () {
        var isFocused = false;
        if (this.focusNode) {
            this.setFocus(this.focusNode.id);
            return true;
        }
        else {
            return false;
        }
    },
    
    
    endHTML : function () {
        var h = bobj.html;
        
        var hsbarStyle = {
            position : 'absolute',
            bottom : '0px',
            left : '0px',
            height : this.SCROLLBAR_SIZE + 'px',
            display : 'none'
        };

        var hsbar_html = h.DIV( {
            id : this.id + '_hsbar',
            style : hsbarStyle
        }, this.hsbar.getHTML());
    
        return '</div>'+this.vsbar.getHTML()+hsbar_html+'</div>';
    },
    
    getModalChildren : function () {
        return this._modalChildren;
    },
    
    /**
     * Adds a child widget as a group tree node.
     * 
     * @param widget
     *            [Widget] Child tree node widget
     */
    addChild : function(widget) {
        var Base = MochiKit.Base;
        var Signal = MochiKit.Signal;
        var connect = Signal.connect;

        widget.expandPath = this._children.length + '';
        this._children.push (widget);

        this.add (widget);
        widget.updatePropertyAndConnectSignals (this.enableDrilldown, this.enableNavigation);

        this._curSigs.push (connect (widget, 'grpDrilldown', Base.partial (Signal.signal, this, 'grpDrilldown')));
        this._curSigs.push (connect (widget, 'grpNodeRetrieveChildren', Base.partial (Signal.signal, this, 'grpNodeRetrieveChildren')));
    },
    
    /**
     * Since GroupTree nodes are delay loaded, we would have to store the timeout ids to cancel them in case user drill down to another views
     */
    delayedBatchAdd : function(children) {
        this._modalChildren = [];
        if (!children || children.length == 0)
            return;

        this._modalChildren = children;
        var childrenHTML = "";

        // Add non group node (i.e. main report and sub report node)
        var rootNode = null;
        var groupRootNode = null;
        while (this.containSingleReportNode(children))
        {
            var childWidget = bobj.crv.createWidget (children[0], true); // true = skip children 
            if (groupRootNode != null)
            {
                groupRootNode.addChild(childWidget);
            }
            else
            {
                this.focusNode = childWidget;
                rootNode = childWidget;
            }
            groupRootNode = childWidget;
            children = children[0].children;
        }

        var numChildrenToRender = 0;
        if (children) {
            numChildrenToRender = children.length > 100 ? 100 : children.length;

            if (groupRootNode != null)
            	groupRootNode.actualNumChildren = children.length;
            else if (rootNode != null)
            	rootNode.actualNumChildren = children.length;
        }
        
        if(numChildrenToRender > 0) {
            for(var i = 0 ; i < numChildrenToRender ; i++) {
                var childWidget = bobj.crv.createWidget (children[i]);
                if (groupRootNode != null)
                {
                    groupRootNode.addChild(childWidget);
                }
                else
                {
                    this.addChild(childWidget);
                    if(this.initialized())
                        childrenHTML += child.getHTML(0);
                }
            }
        }

        if (rootNode != null)
        {
            this.addChild(rootNode);
            childrenHTML += rootNode.getHTML(0, true);
        }
        
        if(this.initialized()) {
            this.appendChildrenHTML (childrenHTML);
            this.initChildren ();
        }
    },
    
    getCtrNode : function () {
        return this._ctrNode;
    },
    
    appendChildrenHTML : function (childrenHTML) {
        append (this.treeLyr, childrenHTML);
    },
    
    appendToTreeCtr : function (node) {
        this._ctrNode.appendChild (node);
    },
    
    init : function() {
        this.initOld ();
        bobj.setVisualStyle (this.layer, this.visualStyle);
        this.css.verticalAlign = "top";

        this.initChildren ();
        
        this._ctrNode = getLayer (this.id + '_ctr');
        
        this.vsbar.init();
        this.vsbar.setScrollableElement (this._ctrNode);

        this.hsbar.init();
        this.hsbar.setScrollableElement (this._ctrNode);
        
        this._hsbarNode = getLayer(this.id + '_hsbar');

        
        this._groupTreeListener = new bobj.crv.GroupTreeListener(this);
        
        this.selectPath(this.selectedPath);
    },
    
    update : function(update) {
        if (update.cons == "bobj.crv.newGroupTree") {
            var args = update.args;
            var path = args.lastExpandedPath;
            var previousFocusPath = this.focusNode ? this.focusNode.expandPath : "";
            /* if path specified, then update specific path, otherwise recreate grouptree */
            // if user has expands a node after session timeout -> the whole gt must be rerendered
            if (path.length > 0 && this._children.length > 0) { 
                this.updateNode (path, update);
            } else {
                this.refreshChildNodes (update);
                
            	if(previousFocusPath != "") {
                    // Accessibility requirement: when a node is expanded, set the focus back to that node after refreshing the tree 
                    var resetFocusNode = this.findNodeByGroupPath(previousFocusPath);
                    if(resetFocusNode != null && (resetFocusNode.domElem.clientWidth != 0 || resetFocusNode.domElem.offsetTop != 0)) {
                        this.setFocus(resetFocusNode.id);
                    }
                    
                }
                
            }
            this.selectPath(args.selectedPath);
        }
    },
    
    selectPath : function(path) {
        if (path) {
            var node = this.findNodeByGroupPath(path);
            if (node && node.selectOld) {
                node.selectOld();
            }
        }
    },
        
    delayedAddChild : function(widget) {
        this.addChild (widget);
        append (this.treeLyr, widget.getHTML (this.initialIndent));
    },
    
    initChildren : function () {
        while(this._lastNodeIdInitialized < this.getChildrenCount() - 1)
            this.initNextChild ();
    },

    initNextChild : function () {
        var nextNode = null;
        var nextNodeId = -1;
        var children = this._children;
        if(this._lastNodeIdInitialized == -1) {
            var treeSpanLayer = getLayer("treeCont_" + this.id);
            nextNode = treeSpanLayer.firstChild;
            nextNodeId = 0;
        }
        else {
            nextNode = this._lastNodeInitialized;
            do {
                if (nextNode.nextSibling != null)
                    nextNode = nextNode.nextSibling;
                else
                    nextNode = nextNode.firstChild;
            } while(!(nextNode.id && nextNode.id.indexOf("TWe_") > -1))
            nextNodeId = this._lastNodeIdInitialized + 1;
        }
        
        if(nextNode != null) {
            this.getChildren(nextNodeId).init(nextNode);
            this._lastNodeInitialized = nextNode;
            this._lastNodeIdInitialized = nextNodeId; 
        }
    },
    
    getBestFitHeight : function () {
        return bobj.getHiddenElementDimensions (this.layer).h;
        /**
         * Since container of tree could be invisible, getHiddenElementDimensions has to be called
         * instead of element.getHeight()
         */
    },

    /**
     * refreshes group tree by removing all nodes and adding new ones
     */
    refreshChildNodes : function(update) {
        this.dispose ();
        this.delayedBatchAdd (update.children);
        MochiKit.Signal.signal(this, "refreshed");
    },
   
    /**
     * @param path
     *            path to the node that should be updated eg) 0-1-2
     * @param newTree
     *            the new tree sent in update
     * 
     * Updates children of node specified by path
     */
    updateNode : function(path, newTree) {
        if (path && path.length > 0) {
            var pathArray = path.split ('-');
            var node = this;
            var newNode = newTree;

            /* Navigating to the node that requires update */
            var i = 0;
            var len = pathArray.length;
            for ( ; i < len; i++) {
                if (node && newNode) {
                    var childIndex = parseInt (pathArray[i]);
                    var newNodeTmp = newNode.children[childIndex];
                    var nodeTmp = node._children[childIndex];
                    if (newNodeTmp && nodeTmp) {
                        newNode = newNodeTmp;
                        node = nodeTmp;
                    }
                } else {
                    break;
                }
            }

            /* if we found the node that requires update, then update its children */
            if (node && newNode && newNode.args.groupPath == node.groupPath && node._children.length == 0) {
                for ( var nodeNum in newNode.children) {
                    var newChildnode = bobj.crv.createWidget (newNode.children[nodeNum]);
                    node.addChild (newChildnode);
                }

                node.updatePropertyAndConnectSignals (this.enableDrilldown, this.enableNavigation);
                node.expand ();
                
                for ( ; i < len; i++) {
                    var childIndex = parseInt (pathArray[i]);
                    var node = node._children[childIndex];
                    if (node)
                        node.expand ();
                }
            }
        }
    },

    containSingleReportNode : function (children) {
        if (!children || children.length != 1)
            return false;
	
        var object = children[0];
	if (object.widgetType)
	    return (object.type != "group");
	else if (object.args)
	    return (object.args.type != "group");
	else
	    return false;
    },
    
    getChildrenCount : function () {
        var count = 0;
        var children = this._children;
        while (this.containSingleReportNode(children))
        {
            count++;
            children = children[0]._children;
        }
        if (children)
            count += children.length;
        return count;
    },

    getModalChildrenCount : function () {
        var count = 0;
        var children = this._modalChildren;
        while (this.containSingleReportNode(children))
        {
            count++;
            children = children[0].children;
        }
        if (children)
            count += children.length;
        return count;
    },

    getChildren : function (index) {
        var children = this._children;
        while (this.containSingleReportNode(children) && index > 0)
        {
            index--;
            children = children[0]._children;
        }
        return children[index];
    },
    
    getModalChild : function (index) {
        var children = this._modalChildren;
        while (this.containSingleReportNode(children) && index > 0)
        {
            index--;
            children = children[0].children;
        }
        if (index < children.length)
        	return children[index];
        else
        	return null;
    },

    delayedAddChildToRealGroupRoot : function (node) {
        this.getRealGroupRoot().delayedAddChild(node);
    },
    
    getRealGroupRoot : function () {
        var children = this._children;
        var root = null;
        while (this.containSingleReportNode(children))
        {
            root = children[0];
            children = children[0]._children;
        }
        return root;
    },

    addRealGroupChildrenHTML : function (html) {
        var treeSpanLayer = getLayer("treeCont_" + this.id);
        var children = this._children;
        var realGroupChildrenLayer = treeSpanLayer;
        
        while(this.containSingleReportNode(children))
        {
            children = children[0]._children;
            realGroupChildrenLayer = realGroupChildrenLayer.firstChild.nextSibling;
        }

        append(realGroupChildrenLayer, html);
    },
    
    /**
     * Private. Callback function when a (complete) group tree node is collapsed.
     */
    _collapse : function(expandPath) {
        MochiKit.Signal.signal (this, 'grpNodeCollapse', expandPath);
        this._doLayout (this.getWidth(), this.getHeight());
    },
    
    /**
     * Private. Callback function when a (complete) group tree node is expanded.
     */
    _expand : function(expandPath) {
        MochiKit.Signal.signal (this, 'grpNodeExpand', expandPath);
        this._doLayout (this.getWidth(), this.getHeight());
    },
    
    _doLayout : function (w, h) {
        var ctrHeight = this.treeLyr.offsetHeight + this.padding;
        var ctrWidth = this.treeLyr.offsetWidth + this.padding;
        var sbarSize = this.SCROLLBAR_SIZE;
        
        bobj.setOuterSize (this.layer, w, h);
        
        var displayHsbar = (ctrWidth > w);
        var displayVsbar = (ctrHeight > h);
        
        if (displayHsbar)
        {
            h -= sbarSize;
            displayVsbar = (ctrHeight > h);
        }

        if (displayVsbar)
        {
            w -= sbarSize;
            if (!displayHsbar && ctrWidth > w)
            {
                displayHsbar = true;
                h -= sbarSize;
            }
        }
        
        
        h = Math.max(0, h);
        w = Math.max(0, w);

        this.vsbar.layer.style.display = displayVsbar ? 'block' : 'none';
        this.vsbar.layer.style.height = h + 'px';
        
        this._hsbarNode.style.display = displayHsbar ? 'block' : 'none';
        this._hsbarNode.style.width = w + 'px';

        if (displayHsbar) {
            this.hsbar.adjustForResize();
        }
        else {
            this._ctrNode.style.left = '0px';
        }        

        if (displayVsbar) {
            this.vsbar.adjustForResize (h);
        }

        bobj.setOuterSize (this._ctrNode, ctrWidth, h);
        
    },
    
    resize : function(width, height) {
        this._doLayout(width, height);
        MochiKit.Signal.signal(this, "resized");
    },
    
    findNodeByGroupPath : function(groupPath) {
        var pathArray = groupPath.split ('-');
        var node = this;
        for ( var i = 0, len = pathArray.length; i < len; i++) {
            if (node) {
                var childIndex = parseInt (pathArray[i]);
                node = node._children[childIndex];
            } else {
                break;
            }
        }
        return node;
    }
};
