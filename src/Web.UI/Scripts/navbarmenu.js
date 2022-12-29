function NavBarMenu(varName, menuSelector) {

	// Private variables ---------------------------------------------------------------------------

	var self          = this;
	var openTimeout   = 100;
	var closeTimeout  = 500;
	var openTimer     = 0;
	var closeTimer    = 0;
	var openMenuItem  = 0;
	var hoverMenuItem = 0;
	var myName        = varName;			 // Name of "this"'s variable
	var selector      = menuSelector;

	// Private methods -----------------------------------------------------------------------------

	var CancelTimer = function () {
		if (openTimer) {
			window.clearTimeout(openTimer);
			openTimer = null;
		}
	}

	var OpenMenu = function () {
		CancelTimer();
		CloseMenu();

		// hoverMenuItem is the item the cursor is currently over. it's the candidate for having its
		// submenu opened.
		if (hoverMenuItem) 
			openMenuItem = $(hoverMenuItem).addClass('open');
	}

	var CloseMenu = function () {

		// openMenuItem is the item whose submenu is currently open. We only close this menu if the 
		// cursor isn't over this, or any of this item's children.
		if (openMenuItem) {

			// This or a child of this is under the cursor? If so, don't close.
			if (openMenuItem.attr('over') || openMenuItem.find('[over=true]').length > 0)
				return;

			openMenuItem.removeClass('open');

			// This item may be a submenu of a menu and so if this item is being closed, we also
			// need to check, and close, all parent menus of this item that aren't currently under
			// the cursor.
			openMenuItem.parents('[over!=true].open').removeClass('open');
		}

		// We've closed this item, and possible parent items. We should now traverse up the 
		// hierarchy of menu items to see if there's a parent item that is open under the cursor. If
		// so, remember this as the currently opened item so we can close it when another item is
		// hovered over.
		if (openMenuItem) {
			var openParents = openMenuItem.parents('[over=true]');
			openMenuItem = (openParents.length > 0)? $(openParents[0]) : null;
		}
	}

	// Delay the open so that a cursor transiting over the menu doesn't accidentally open it.
	var ScheduleOpen = function () {
		hoverMenuItem = this;	// Candidate for having it's menu opened
		openTimer     = window.setTimeout(OpenMenu, openMenuItem? 0 : openTimeout);
	}

	// Delay the close so that a cursor accidentally leaving the menu doesn't prematurely close it.
	var ScheduleClose = function () {
		hoverMenuItem = 0;
		closeTimer    = window.setTimeout(CloseMenu, closeTimeout);
	}

	// Public methods ------------------------------------------------------------------------------

	if ($ && $.support.opacity) // Means everyone except IE8 and below.
		$(document).ready(function () { eval(myName + '.InitMenu();'); }).click(self.CloseMenu);

	this.InitMenu = function () {
	    if (!navigator.msMaxTouchPoints) {
		    $(selector + '.openable').removeClass('openable').hover(ScheduleOpen, ScheduleClose);
		    $(selector).hover(function (e) { $(this).attr('over', 'true'); },
					          function (e) { $(this).removeAttr('over'); });
        }
	}

	// Specific to the Article dropdown. Included here just to keep it out of harm's way.
	this.ShowMap = function (prnt, elmId) {
		var map = $('#' + elmId);

		if (!prnt.populated && !prnt.populating) {
			prnt.populating = true;
			map.css({ 'height': '200px', 'width': '100px' });
			map.load("/script/content/Ajax/SiteMap.aspx", function () { prnt.populated = true; });
			window.setTimeout(this.InitMenu, 200); // process new additions to menu so they are openable.
			prnt.populated = true;
			prnt.populating = false;
		}

		if (prnt.populated) {
			this.InitMenu(); //  Just in case it didn't work the first time.

			map.css('margin', '0');
			map.css('width', 'auto');
			map.css('height', 'auto');
		}
	}

}

var navBarMenu = new NavBarMenu('navBarMenu', '.navmenu li');
var signinMenu = new NavBarMenu('signinMenu', '.member-signin');
// Copyright (c) 2008, The Code Project. All rights reserved.
/// <reference path="jquery-1.3.2-vsdoc2.js" />

// Initialize watermark on input element.
function InitWatermark(inputId, label) {
	var inputObj = $('#' + inputId);
	if (!inputObj) return;

	inputObj.blur(function() { UpdateWatermark(this, label); });
	inputObj.focus(function() { UpdateWatermark(this, label); });

	var text = $.trim(inputObj.val());
	if (text == '' || text == label) {
		inputObj.val(label);
		inputObj.addClass('subdue');
	}
	else if (text != '' && text != label)
		inputObj.removeClass('subdue');
}

// Update the watermark as text changes within the input element.
function UpdateWatermark(inputObj, label) {
	if (!inputObj) return;

	var text = $.trim($(inputObj).val());
	if (text == label && $(inputObj).hasClass('subdue')) {
		$(inputObj).val('');
		$(inputObj).removeClass('subdue');
	}
	else if (text == '' && !$(inputObj).hasClass('subdue')) {
		$(inputObj).val(label);
		$(inputObj).addClass('subdue');
	}
	else
		$(inputObj).removeClass('subdue');
}

// Makes an element stick to the top of a page on scroll:
//
//   $(elm).MakeSticky("sticky_div", "stop_div", "top");    // only "top" supported so far.

(function ($) {

    $.sticky = function (stickyElement, $stopElement, stickyPos) {

        // To avoid scope issues, use 'self' instead of 'this' to reference this class from internal 
        // events and functions.
        var self = this;

        var $stickyElement = $(stickyElement);
        var padding        = 10;
        var isStuck        = false;
        var originalLayout;

        function GetOriginalLayout() {
            if ($stickyElement) {

                var position = $stickyElement.offset();

                var original = new Object();
                original.isStickyRequired = stickyPos === "top";
                original.top      = position.top;
                original.left     = position.left;
                original.position = $stickyElement.css('position');

                // if one of the coords is negative return null to identify that we can't sticky
                if (original.top === -1)
                    return null;

                return original;
            }
        };

        function ResetLayout() {
            if (originalLayout) {
                $stickyElement.css('position', originalLayout.position);
                $stickyElement.css('top',      originalLayout.top);
                $stickyElement.css('left',     originalLayout.left);
                $stickyElement.removeClass("stuck");
                isStuck = false;
            }
        }

        self.ReGetOriginalLayout = function () {
            if (originalLayout) {

                // Position the element back normally
                ResetLayout();

                // recreate the settings to account for possible absolute position changes
                originalLayout = GetOriginalLayout();

                // apply new settings
                self.RepositionStickyElement();
            }
        };

        self.RepositionStickyElement = function () {

            var stopBounds;
            if ($stopElement && $stopElement.length)
                stopBounds = getBoundingRect($stopElement); 

            var scrollTop    = $(window).scrollTop();
            var stickyBounds = getBoundingRect($stickyElement); 

            var stickTop;
            if (stopBounds && stickyBounds.bottom > stopBounds.top) {
                stickTop = stickyBounds.top - (stickyBounds.bottom - stopBounds.top) - scrollTop;
            } else if (scrollTop + padding >= originalLayout.top)
                stickTop = padding;
            else if (!isStuck)
                originalLayout = GetOriginalLayout();

            // PROBLEM: fixed elements are not being identified as fixed
            // if (originalLayout.position == 'fixed' && stickyBounds.bottom < stopBounds.top)
            //    stickTop = 0; // Leave already fixed elements where they were if above bottom stop.

            // If the element had previously hit a stop element then it would have been positioned
            // such that it wasn't overlapping the stop element. If we're now thinking we can set it
            // back to the top of the frame then we need to double-check
            if (stickTop && stopBounds && (scrollTop + stickTop + stickyBounds.height) > stopBounds.top)
                stickTop = stickyBounds.top - (stickyBounds.bottom - stopBounds.top) - scrollTop;

            if (stickTop) {

                // self.ReGetOriginalLayout(true);

                $stickyElement.css('position', 'fixed');
                $stickyElement.css('top',      stickTop);
                $stickyElement.css('left',     originalLayout.left -$(window).scrollLeft());
                $stickyElement.addClass("stuck");

                isStuck = true;

                // Make sure we're not pushing the sticky element above the original location.
                /* COMMENTED: (CM) slows things down and crashes IE. Awesome.
                stickyBounds = getBoundingRect($stickyElement);
                if (stickyBounds.top <= originalLayout.top)
                    ResetLayout(); 
                */
            }
            else
                ResetLayout();
        };

        function getBoundingRect($element) {
            var bounds = $element.offset();

            bounds.width  = $element.outerWidth(true); // true = include margins in the value
            bounds.height = $element.outerHeight(true);

            bounds.right  = bounds.left + bounds.width;
            bounds.bottom = bounds.top  + bounds.height;

            // Not needed.
            // bounds.marginVertical   = (bounds.height - $element.outerHeight(false)) / 2;
            // bounds.marginHorizontal = (bounds.width  - $element.outerWidth(false))  / 2;

            return bounds;
        }

        self.init = function () {

            stickyPos = stickyPos || "top";

            if ($stickyElement) {
                originalLayout = GetOriginalLayout();

                $(window).bind('resize', self.ReGetOriginalLayout);
                $(window).bind('scroll', self.RepositionStickyElement);

                // Looks crappy. The content to be fixed slides underneath the newly fixed content
                /*
                if ('ontouchmove' in window)
                    $(window).bind('touchmove', self.RepositionStickyElement);
                */
            }
        }

        self.init();
    };

    // Returns enhanced elements that will fix to the top of the page when the
    // page is scrolled.
    $.fn.sticky = function ($stopElement, stickyPos) {
        return this.each(function () {
            (new $.sticky(this, $stopElement, stickyPos));
        });
    };

})(jQuery);
function NotificationAlert(parentId, elementId, ajaxPageUrl) {

	// Public variables ----------------------------------------------------------------------------

	// this.updateMessageUrl = ''; 		 // The URL to call when updating the message

	// Private variables ---------------------------------------------------------------------------

	var self         = this;
	var _parent      = $('#' + parentId);
	var _element     = $('#' + elementId);
	var _ajaxPageUrl = ajaxPageUrl;

	// Sets up a flyout menu with the flag options. Recreated each time - may want to create once
	// and store if dynamic creation not needed.
	var setupAlertBox = function () {
		if (!_parent.populated && !_parent.populating) {
			_parent.populating = true;
			_element.load(_ajaxPageUrl, {cache:false}, function () { _parent.populated = true; });
			_parent.populated = true;
			_parent.populating = false;
		}
	}

	// Call service url to update article status.
	this.MarkAsRead = function (notificationId, serviceUrl, linkId, containerId) {

		var link = $('#' + linkId);
		var oldHtml = link.html();
		link.html('<img src="/Images/animated_loading_blue.gif" align="absmiddle" style="border:0" />');

		$.ajax(
		{
			type:		'POST',
			cache:		false,
			url:		serviceUrl,
			data:		'{notificationId: ' + notificationId + '}',
			contentType: 'application/json; charset=utf-8',
			dataType:	'json',
			tryCount:	0,
			retryLimit: 3,
			timeout:	5000,

			success: function () {
				link.remove();
				$('#' +containerId).children().first().removeClass('bold');	
			},
			error: function () {
				link.html(oldHtml);		
			}
		});

		return false;
	}

	// Setup everything.
	this.initialise = function () {
		$(_parent).bind('mouseover', setupAlertBox);
	}
}

/*
var notificationAlert;
$(document).ready(function () {
	notificationAlert = new NotificationAlert(parent, 'elementId', 'http://mydomain.com/ajax');
	notificationAlert.initialise();
});
*/