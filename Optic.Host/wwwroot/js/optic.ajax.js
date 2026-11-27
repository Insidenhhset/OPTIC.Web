
(function () {
    "use strict";

    const userTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
    const loader = document.getElementById("opticLoader");
    const content = document.getElementById("main-content");
    const pageTitle = document.getElementById("pageTitle");
    const toast = document.getElementById("opticToast");

    let activeRequests = 0;
    let toastTimer = null;

    // -----------------------------
    // Global loader
    // -----------------------------

    function showLoader() {
        activeRequests++;

        if (loader) {
            loader.classList.add("active");
        }
    }

    function hideLoader() {
        activeRequests = Math.max(0, activeRequests - 1);

        if (activeRequests === 0 && loader) {
            loader.classList.remove("active");
        }
    }

    // -----------------------------
    // Notifications
    // -----------------------------

    function showMessage(message, type = "error") {
        if (!toast) return;

        toast.textContent = message;

        toast.style.background =
            type === "success" ? "#166534" : "#991b1b";

        toast.style.display = "block";

        clearTimeout(toastTimer);

        toastTimer = setTimeout(function () {
            toast.style.display = "none";
        }, 3500);
    }

    // -----------------------------
    // Authentication handling
    // -----------------------------

    function handleAuthenticationError(status) {

        if (status === 401) {

            const returnUrl =
                window.location.pathname + window.location.search;

            window.location.replace(
                "/Account/Login?returnUrl=" +
                encodeURIComponent(returnUrl)
            );

            return true;
        }

        if (status === 403) {
            window.location.href = "/Account/AccessDenied";
            return true;
        }

        return false;
    }

    // -----------------------------
    // Update page content
    // -----------------------------

    function updatePage(html, url) {
        const parser = new DOMParser();

        const documentObject = parser.parseFromString(
            html,
            "text/html"
        );

        const page = documentObject.querySelector("[data-ajax-page]");

        if (!page) {
            throw new Error(
                "Invalid AJAX response. Expected a partial page."
            );
        }

        content.innerHTML = page.outerHTML;

        if (typeof window.initializeApplicationMappings === "function") {
            window.initializeApplicationMappings();
        }

        const title =page.getAttribute("data-page-title") || "OPTIC";

        if (pageTitle) {
            pageTitle.textContent = title;
        }

        document.title = title + " | OPTIC";

        updateActiveNavigation(url);

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    }

    // -----------------------------
    // Sidebar active navigation
    // -----------------------------


    function updateActiveNavigation(url) {
        const currentUrl = new URL(url, window.location.origin);
        const currentPath = currentUrl.pathname.toLowerCase();

        // Update active state for all sidebar links.
        document.querySelectorAll("a[data-ajax-nav]").forEach(function (link) {
            const linkUrl = new URL(link.href, window.location.origin);
            const linkPath = linkUrl.pathname.toLowerCase();

            const isActive =
                linkPath === currentPath ||
                (linkPath !== "/" && currentPath.startsWith(linkPath + "/"));

            link.classList.toggle("active", isActive);
        });

        // Handle Role Management submenu.
        const roleManagementLink = Array.from(
            document.querySelectorAll("a[data-ajax-nav]")
        ).find(function (link) {
            return link.getAttribute("href")
                ?.toLowerCase() === "/identityroles/index";
        });

        if (!roleManagementLink) return;

        const roleSubmenu = roleManagementLink.nextElementSibling;

        if (!roleSubmenu ||
            !roleSubmenu.classList.contains("sidebar-submenu")) {
            return;
        }

        const isRoleSectionActive =
            currentPath.startsWith("/identityroles");

        roleManagementLink.classList.toggle(
            "active",
            isRoleSectionActive
        );

        roleSubmenu.style.display =
            isRoleSectionActive ? "" : "none";
    }


    // -----------------------------
    // AJAX page navigation
    // -----------------------------

    async function navigate(url, addHistory = true) {
        const target = new URL(url, window.location.origin);

        if (target.origin !== window.location.origin) {
            window.location.href = target.href;
            return;
        }

        showLoader();

        try {
            const response = await fetch(target.href, {
                method: "GET",

                credentials: "same-origin",

                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "text/html",
                    "X-Time-Zone": userTimeZone
                }
            });

            if (handleAuthenticationError(response.status)) {
                return;
            }

            if (!response.ok) {
                throw new Error(
                    "Unable to load page. HTTP " + response.status
                );
            }

            const html = await response.text();

            updatePage(html, target.href);

            if (addHistory) {
                window.history.pushState(
                    { opticAjax: true },
                    "",
                    target.href
                );
            }

        } catch (error) {
            console.error("OPTIC navigation error:", error);

            showMessage(
                error.message || "Unable to load the requested page."
            );

        } finally {
            hideLoader();
        }
    }

    // -----------------------------
    // AJAX form submission
    // -----------------------------

    async function submitAjaxForm(form) {
        const method = (
            form.getAttribute("method") || "POST"
        ).toUpperCase();

        const url = form.getAttribute("action") ||
            window.location.href;

        const formData = new FormData(form);

        let requestUrl = url;
        let requestBody = formData;

        if (method === "GET") {
            const query = new URLSearchParams();

            formData.forEach(function (value, key) {
                if (typeof value === "string") {
                    query.append(key, value);
                }
            });

            const target = new URL(
                url,
                window.location.origin
            );

            target.search = query.toString();

            requestUrl = target.href;
            requestBody = undefined;
        }

        showLoader();

        try {
            const response = await fetch(requestUrl, {
                method: method,

                body: method === "GET" ? undefined : requestBody,

                credentials: "same-origin",

                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json, text/html",
                    "X-Time-Zone": userTimeZone
                }
            });

            if (handleAuthenticationError(response.status)) {
                return;
            }

            const contentType =
                response.headers.get("content-type") || "";

            if (contentType.includes("application/json")) {
                const result = await response.json();

                if (!response.ok) {
                    throw new Error(
                        result.message || "Request failed."
                    );
                }

                if (result.redirectUrl) {
                    await navigate(result.redirectUrl);
                    return;
                }

                if (result.reloadContentUrl) {
                    await navigate(result.reloadContentUrl, false);
                }

                if (result.message) {
                    showMessage(result.message, "success");
                }

                return;
            }

            const html = await response.text();

            // A validation response can return HTML with HTTP 400.
            if (
                contentType.includes("text/html") &&
                (response.ok || response.status === 400)
            ) {
                updatePage(html, window.location.href);
                return;
            }

            throw new Error(
                "Request failed. HTTP " + response.status
            );

        } catch (error) {
            console.error("OPTIC form error:", error);

            showMessage(
                error.message || "Unable to process your request."
            );

        } finally {
            hideLoader();
        }
    }

    // -----------------------------
    // AJAX navigation click handler
    // -----------------------------

    document.addEventListener("click", function (event) {
        const link = event.target.closest("a[data-ajax-nav]");

        if (!link) return;

        if (
            event.ctrlKey ||
            event.shiftKey ||
            event.altKey ||
            event.metaKey ||
            link.target === "_blank" ||
            link.hasAttribute("download")
        ) {
            return;
        }

        const url = new URL(link.href, window.location.origin);

        if (url.origin !== window.location.origin) {
            return;
        }

        event.preventDefault();

        if (url.href === window.location.href) {
            return;
        }

        navigate(url.href);
    });

    // -----------------------------
    // AJAX form handler
    // -----------------------------

    document.addEventListener("submit", function (event) {
        const form = event.target.closest("form[data-ajax-form]");

        if (!form) return;

        event.preventDefault();

        submitAjaxForm(form);
    });

    // -----------------------------
    // Browser back / forward
    // -----------------------------

    window.addEventListener("popstate", function () {
        navigate(window.location.href, false);
    });


    // -----------------------------
    // Absolute session countdown
    // -----------------------------

    (function initializeSessionCountdown() {

        const element = document.getElementById(
            "opticSessionCountdown"
        );

        if (!element) return;

        const expiryTime = Number(
            element.dataset.expiryUnixMs
        );

        const serverTime = Number(
            element.dataset.serverNowUnixMs
        );

        if (
            !Number.isFinite(expiryTime) ||
            !Number.isFinite(serverTime) ||
            expiryTime <= 0
        ) {
            redirectToLogin();
            return;
        }

        const initialRemainingMs = expiryTime - serverTime;

        if (initialRemainingMs <= 0) {
            redirectToLogin();
            return;
        }

        const performanceStart = performance.now();
        const dateStart = Date.now();

        let redirecting = false;

        let countdownInterval = null;

        let expiryChannel = null;

        try {
            expiryChannel = new BroadcastChannel(
                "optic-session-events"
            );

            expiryChannel.onmessage = function (event) {
                if (event.data === "SESSION_EXPIRED") {
                    redirectToLogin(false);
                }
            };
        }
        catch (error) {
            console.warn(
                "Cross-tab session communication unavailable."
            );
        }

        function redirectToLogin(broadcast = true) {

            if (redirecting) return;

            redirecting = true;

            if (countdownInterval) {
                clearInterval(countdownInterval);
            }

            element.textContent = "Session expired";

            if (broadcast && expiryChannel) {
                expiryChannel.postMessage("SESSION_EXPIRED");
            }

            window.location.replace(
                "/Account/Login?reason=session-expired"
            );
        }

        function getRemainingMilliseconds() {

            const performanceElapsed =
                performance.now() - performanceStart;

            const dateElapsed =
                Date.now() - dateStart;

            // Use the greater elapsed duration so that
            // a backward system-clock adjustment cannot
            // extend the session.
            const elapsed = Math.max(
                performanceElapsed,
                dateElapsed
            );

            return initialRemainingMs - elapsed;
        }

        function updateCountdown() {

            if (redirecting) return;

            const remainingMs = getRemainingMilliseconds();

            if (remainingMs <= 0) {
                redirectToLogin();
                return;
            }

            const remainingSeconds = Math.ceil(
                remainingMs / 1000
            );

            const minutes = Math.floor(
                remainingSeconds / 60
            );

            const seconds = remainingSeconds % 60;

            element.textContent =
                `Session expires in ${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;

            const warning = remainingSeconds <= 60;

            element.classList.toggle(
                "bg-danger",
                warning
            );

            element.classList.toggle(
                "bg-warning",
                !warning
            );

            element.classList.toggle(
                "text-dark",
                !warning
            );

            element.classList.toggle(
                "text-white",
                warning
            );
        }

        updateCountdown();

        countdownInterval = setInterval(
            updateCountdown,
            250
        );

        // Browser tab becomes active again.
        document.addEventListener(
            "visibilitychange",
            function () {
                if (!document.hidden) {
                    updateCountdown();
                }
            }
        );

        // Browser restores a page from its back-forward cache.
        window.addEventListener(
            "pageshow",
            updateCountdown
        );

        window.addEventListener(
            "focus",
            updateCountdown
        );

        // Browser goes offline: countdown still runs.
        window.addEventListener(
            "online",
            updateCountdown
        );

    })();


    // -----------------------------
    // Expose public API
    // -----------------------------

    window.OpticAjax = {
        navigate: navigate,
        showLoader: showLoader,
        hideLoader: hideLoader,
        showMessage: showMessage
    };

})();
