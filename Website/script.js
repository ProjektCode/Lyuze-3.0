/**
 * Lyuze Website Scripts
 * Handles avatar loading and mobile navigation
 */

(function() {
    'use strict';

    /**
     * Initialize avatar with loading and error handling
     */
    function initAvatar() {
        const avatar = document.getElementById('navAvatar');
        
        if (!avatar) {
            return;
        }
        
        // Add error handling for avatar
        avatar.addEventListener('error', function() {
            // Fallback: hide avatar on error
            this.style.display = 'none';
        });
        
        // Add loading state
        avatar.addEventListener('load', function() {
            this.style.opacity = '1';
        });
        
        // Set initial opacity for smooth fade-in
        avatar.style.opacity = '0';
        avatar.style.transition = 'opacity 0.3s ease';
    }

    /**
     * Initialize mobile navigation menu
     */
    function initMobileMenu() {
        const navToggle = document.getElementById('navToggle');
        const navMenu = document.getElementById('navMenu');
        
        if (!navToggle || !navMenu) {
            return;
        }
        
        // Toggle menu on button click
        navToggle.addEventListener('click', function() {
            const isExpanded = navToggle.getAttribute('aria-expanded') === 'true';
            navToggle.setAttribute('aria-expanded', !isExpanded);
            navMenu.classList.toggle('active');
        });
        
        // Close menu when clicking on a link
        const navLinks = navMenu.querySelectorAll('a');
        navLinks.forEach(function(link) {
            link.addEventListener('click', function() {
                navToggle.setAttribute('aria-expanded', 'false');
                navMenu.classList.remove('active');
            });
        });
        
        // Close menu when clicking outside
        document.addEventListener('click', function(event) {
            const isClickInsideNav = navMenu.contains(event.target) || navToggle.contains(event.target);
            
            if (!isClickInsideNav && navMenu.classList.contains('active')) {
                navToggle.setAttribute('aria-expanded', 'false');
                navMenu.classList.remove('active');
            }
        });
        
        // Close menu on escape key
        document.addEventListener('keydown', function(event) {
            if (event.key === 'Escape' && navMenu.classList.contains('active')) {
                navToggle.setAttribute('aria-expanded', 'false');
                navMenu.classList.remove('active');
            }
        });
    }

    /**
     * Initialize all functionality when DOM is ready
     */
    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                initAvatar();
                initMobileMenu();
            });
        } else {
            // DOM is already ready
            initAvatar();
            initMobileMenu();
        }
    }

    // Start initialization
    init();
})();
