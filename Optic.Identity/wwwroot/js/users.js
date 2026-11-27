
(function () {
    "use strict";

    const API = "/IdentityUsers";
    const userTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
    const MAX_PROFILE_SIZE = 2 * 1024 * 1024;
    const ALLOWED_IMAGE_TYPES = [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    let users = [];
    let selectedDeleteId = null;
    let profilePreviewUrl = null;
    let currentAvatarUrl = null;

    const get = (user, property) =>
        user?.[property] ??
        user?.[property.charAt(0).toLowerCase() + property.slice(1)];

    const escapeHtml = value =>
        String(value ?? "").replace(/[&<>"']/g, character => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        })[character]);

    function currentPage() {
        return document.getElementById("usersPage");
    }

    function modal(id) {
        const element = document.getElementById(id);

        return element
            ? bootstrap.Modal.getOrCreateInstance(element)
            : null;
    }

    function clearValidationErrors() {

        const form = document.getElementById("userForm");

        if (!form) return;

        form.querySelectorAll(".is-invalid").forEach(element => {
            element.classList.remove("is-invalid");
        });

        form.querySelectorAll(".invalid-feedback").forEach(element => {
            element.textContent = "";
        });

        const errorBox = document.getElementById("userFormError");

        if (errorBox) {
            errorBox.textContent = "";
            errorBox.classList.add("d-none");
        }
    }

    function showFieldError(field, message) {

        if (!field || !message) return false;

        const form = document.getElementById("userForm");

        if (!form) return false;

        // Handles keys such as request.EmployeeCode.
        const fieldName = field.split(".").pop();

        const input = Array.from(form.elements).find(element =>
            element.id?.toLowerCase() === fieldName.toLowerCase()
        );

        if (!input) return false;

        input.classList.add("is-invalid");

        const feedback = document.getElementById(`${input.id}-error`);

        if (feedback) {
            feedback.textContent = message;
        }

        return true;
    }

    function showValidationErrors(error) {

        clearValidationErrors();

        let hasFieldErrors = false;

        // Handle custom ProblemDetails field extension.
        if (error.field) {

            hasFieldErrors = showFieldError(
                error.field,
                error.message
            );
        }

        // Handle ASP.NET Core ValidationProblemDetails.
        if (error.errors) {

            Object.entries(error.errors).forEach(([field, messages]) => {

                const message = Array.isArray(messages)
                    ? messages.join(" ")
                    : String(messages);

                if (showFieldError(field, message)) {
                    hasFieldErrors = true;
                }
            });
        }

        // Display general errors when no field could be identified.
        if (!hasFieldErrors) {
            showFormError(error.message);
        }
    }

    class ApiError extends Error {
        constructor(problem, status) {

            const defaultMessages = {
                400: "The request contains invalid data.",
                401: "Your session has expired. Please log in again.",
                403: "You do not have permission to perform this action.",
                404: "The requested resource was not found.",
                409: "This record conflicts with an existing record.",
                500: "An unexpected server error occurred.",
                503: "The service is temporarily unavailable."
            };

            const validationErrors = problem?.errors
                ? Object.values(problem.errors).flat().join(" ")
                : "";

            const message =
                problem?.detail ||
                problem?.message ||
                validationErrors ||
                problem?.title ||
                defaultMessages[status] ||
                "Something went wrong. Please try again.";

            super(message);

            this.name = "ApiError";
            this.status = problem?.status ?? status;
            this.title = problem?.title ?? "";
            this.detail = problem?.detail ?? "";
            this.field = problem?.field ?? null;
            this.errors = problem?.errors ?? {};
        }
    }

    async function request(url, options = {}) {

        const headers = {
            "X-Requested-With": "XMLHttpRequest",
            "X-Time-Zone": userTimeZone,
            ...options.headers
        };

        const isFormData = options.body instanceof FormData;

        if (
            !isFormData &&
            options.body !== undefined &&
            !headers["Content-Type"]
        ) {
            headers["Content-Type"] = "application/json";
        }

        const response = await fetch(url, {
            ...options,
            headers
        });

        const contentType = response.headers.get("content-type") || "";

        let result = null;

        if (contentType.includes("json")) {
            result = await response.json().catch(() => null);
        }

        if (!response.ok) {
            throw new ApiError(result, response.status);
        }

        return result;
    }


    async function loadUsers() {

        const page = currentPage();

        if (!page) return;

        const body = page.querySelector("#usersTableBody");

        if (!body) return;

        body.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-secondary">
                    <span class="spinner-border spinner-border-sm me-2"></span>
                    Loading users...
                </td>
            </tr>`;

        try {

            const result = await request(`${API}/GetAll`);

            users = Array.isArray(result) ? result : [];

            renderUsers(users);

        } catch (error) {

            body.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center text-danger py-4">
                        ${escapeHtml(error.message)}
                    </td>
                </tr>`;
        }
    }

    function getInitials(firstName, lastName) {

        const first = firstName?.trim().charAt(0) || "";
        const last = lastName?.trim().charAt(0) || "";

        return (first + last).toUpperCase() || "?";
    }

    function renderAvatar(user, size = 38) {

        const avatarUrl = get(user, "AvatarUrl");

        const firstName = get(user, "FirstName") || "";
        const lastName = get(user, "LastName") || "";

        const fullName = `${firstName} ${lastName}`.trim();

        const style = `width:${size}px;height:${size}px;`;

        if (avatarUrl) {

            return `
                <img src="${escapeHtml(avatarUrl)}"
                     alt="${escapeHtml(fullName || "User")}"
                     class="rounded-circle flex-shrink-0 object-fit-cover"
                     style="${style}"
                     onerror="this.style.display='none';
                              this.nextElementSibling.classList.remove('d-none');
                              this.nextElementSibling.classList.add('d-flex');">

                <div class="rounded-circle bg-primary-subtle text-primary
                            align-items-center justify-content-center
                            fw-semibold flex-shrink-0 d-none"
                     style="${style}">
                    ${escapeHtml(getInitials(firstName, lastName))}
                </div>`;
        }

        return `
            <div class="rounded-circle bg-primary-subtle text-primary
                        d-flex align-items-center justify-content-center
                        fw-semibold flex-shrink-0"
                 style="${style}">
                ${escapeHtml(getInitials(firstName, lastName))}
            </div>`;
    }

    function releaseProfilePreviewUrl() {

        if (profilePreviewUrl) {

            URL.revokeObjectURL(profilePreviewUrl);

            profilePreviewUrl = null;
        }
    }

    function updateProfilePreview(imageUrl = null) {

        const preview = document.getElementById("profilePreview");

        if (!preview) return;

        const existingImage = preview.querySelector("img");

        if (imageUrl || existingImage) {

            if (imageUrl) {
                preview.innerHTML = `
                <img src="${escapeHtml(imageUrl)}"
                     alt="Profile picture preview"
                     class="w-100 h-100 object-fit-cover">`;
            }

            return;
        }

        const firstName = document.getElementById("FirstName")?.value || "";
        const lastName = document.getElementById("LastName")?.value || "";

        preview.textContent = getInitials(firstName, lastName);
    }

    function renderUsers(data) {

        const page = currentPage();

        if (!page) return;

        const body = page.querySelector("#usersTableBody");
        const count = page.querySelector("#usersCount");

        if (!body || !count) return;

        if (!data?.length) {

            body.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-secondary py-4">
                    No users found.
                </td>
            </tr>`;

            count.textContent = "0 users";

            return;
        }

        body.innerHTML = data.map(user => {

            const id = get(user, "Id");

            const fullName =
                `${get(user, "FirstName") ?? ""} ${get(user, "LastName") ?? ""}`.trim();

            const active = Boolean(get(user, "IsActive"));

            return `
            <tr>

                <td>
                    <div class="d-flex align-items-center gap-2">

                        ${renderAvatar(user, 38)}

                        <div>
                            <div class="fw-semibold">
                                ${escapeHtml(fullName || "-")}
                            </div>

                            <small class="text-secondary">
                                ${escapeHtml(get(user, "EmployeeCode") || "-")}
                            </small>
                        </div>

                    </div>
                </td>

                <td>
                    ${escapeHtml(get(user, "Username") || "-")}
                </td>

                <td>
                    ${escapeHtml(get(user, "Email") || "-")}
                </td>

                <td>
                    ${escapeHtml(get(user, "Mobile") || "-")}
                </td>

                <td>
                    <span class="badge ${active
                    ? "bg-success-subtle text-success"
                    : "bg-secondary-subtle text-secondary"
                }">
                        ${active ? "Active" : "Inactive"}
                    </span>
                </td>

                <td>
                    ${escapeHtml(get(user, "LastLoginAtFormatted") || "Never")}
                </td>

                <td class="text-end text-nowrap">

                    <button type="button"
                            class="btn btn-sm btn-light"
                            data-action="view"
                            data-id="${escapeHtml(id)}"
                            title="View">
                        <i class="bi bi-eye"></i>
                    </button>

                    <button type="button"
                            class="btn btn-sm btn-light"
                            data-action="edit"
                            data-id="${escapeHtml(id)}"
                            title="Edit">
                        <i class="bi bi-pencil"></i>
                    </button>

                    <button type="button"
                            class="btn btn-sm btn-light text-danger"
                            data-action="delete"
                            data-id="${escapeHtml(id)}"
                            title="Deactivate">
                        <i class="bi bi-person-x"></i>
                    </button>

                </td>

            </tr>`;

        }).join("");

        count.textContent = `${data.length} users`;
    }

    function formatDate(value) {

        if (!value) return "Never";

        const date = new Date(value);

        return Number.isNaN(date.getTime())
            ? "-"
            : date.toLocaleString();
    }

    function resetForm() {

        const form = document.getElementById("userForm");

        if (!form) return;

        releaseProfilePreviewUrl();

        currentAvatarUrl = null;

        form.reset();

        document.getElementById("userId").value = "";

        document.getElementById("userModalTitle").textContent = "Add User";

        document.getElementById("passwordGroup").classList.remove("d-none");

        document.getElementById("activeGroup").classList.add("d-none");

        document.getElementById("Password").required = true;

        const errorBox = document.getElementById("userFormError");

        errorBox.textContent = "";

        errorBox.classList.add("d-none");

        updateProfilePreview();
    }

    function openAddModal() {

        resetForm();

        modal("userModal")?.show();
    }

    async function openEditModal(id) {

        try {

            const user = await request(
                `${API}/GetById?id=${encodeURIComponent(id)}`
            );

            resetForm();

            document.getElementById("userId").value =
                get(user, "Id") ?? "";

            document.getElementById("EmployeeCode").value =
                get(user, "EmployeeCode") ?? "";

            document.getElementById("Username").value =
                get(user, "Username") ?? "";

            document.getElementById("FirstName").value =
                get(user, "FirstName") ?? "";

            document.getElementById("LastName").value =
                get(user, "LastName") ?? "";

            document.getElementById("Email").value =
                get(user, "Email") ?? "";

            document.getElementById("Mobile").value =
                get(user, "Mobile") ?? "";

            document.getElementById("EmployeeTypeId").value =
                get(user, "EmployeeTypeId") ?? "";

            document.getElementById("IsActive").checked =
                Boolean(get(user, "IsActive"));

            document.getElementById("userModalTitle").textContent =
                "Edit User";

            document.getElementById("passwordGroup").classList.add("d-none");

            document.getElementById("activeGroup").classList.remove("d-none");

            document.getElementById("Password").required = false;

            // Preserve the existing picture when no replacement is selected.
            currentAvatarUrl = get(user, "AvatarUrl") || null;

            updateProfilePreview(currentAvatarUrl);

            modal("userModal")?.show();

        } catch (error) {

            alert(error.message);
        }
    }

    async function openViewModal(id) {

        try {

            const user = await request(
                `${API}/GetById?id=${encodeURIComponent(id)}`
            );

            const fields = [

                ["User ID", get(user, "Id")],
                ["Employee Code", get(user, "EmployeeCode")],
                ["Username", get(user, "Username")],
                ["First Name", get(user, "FirstName")],
                ["Last Name", get(user, "LastName")],
                ["Email", get(user, "Email")],
                ["Mobile", get(user, "Mobile")],
                ["Employee Type ID", get(user, "EmployeeTypeId")],
                ["Status", get(user, "IsActive") ? "Active" : "Inactive"],

                ["Last Login",
                    get(user, "LastLoginAtFormatted") ??
                    get(user, "lastLoginAtFormatted")],

                ["Password Changed",
                    get(user, "PasswordChangedAtFormatted") ??
                    get(user, "passwordChangedAtFormatted")],

                ["Created At",
                    get(user, "CreatedAtFormatted") ??
                    get(user, "createdAtFormatted")],

                ["Created By", get(user, "CreatedBy")],

                ["Updated At",
                    get(user, "UpdatedAtFormatted") ??
                    get(user, "updatedAtFormatted")],

                ["Updated By", get(user, "UpdatedBy")]

            ];

            document.getElementById("userDetails").innerHTML =
                fields.map(([label, value]) => `
                    <div class="col-md-6">

                        <small class="text-secondary">
                            ${escapeHtml(label)}
                        </small>

                        <div class="fw-medium">
                            ${escapeHtml(value ?? "-")}
                        </div>

                    </div>
                `).join("");

            modal("viewUserModal")?.show();

        } catch (error) {

            alert(error.message);
        }
    }

    async function saveUser(event) {

        event.preventDefault();

        const form = event.target;

        if (!(form instanceof HTMLFormElement)) {
            console.error("User form was not found.");
            return;
        }

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        clearValidationErrors();

        const id = document.getElementById("userId").value;
        const isEdit = Boolean(id);

        const formData = new FormData();

        formData.append(
            "EmployeeCode",
            document.getElementById("EmployeeCode").value.trim()
        );

        formData.append(
            "Username",
            document.getElementById("Username").value.trim()
        );

        formData.append(
            "FirstName",
            document.getElementById("FirstName").value.trim()
        );

        formData.append(
            "LastName",
            document.getElementById("LastName").value.trim()
        );

        formData.append(
            "Email",
            document.getElementById("Email").value.trim()
        );

        formData.append(
            "Mobile",
            document.getElementById("Mobile").value.trim()
        );

        formData.append(
            "EmployeeTypeId",
            document.getElementById("EmployeeTypeId").value
        );

        // Optional profile picture.
        const profilePicture =
            document.getElementById("ProfilePicture").files[0];

        if (profilePicture) {

            if (!ALLOWED_IMAGE_TYPES.includes(profilePicture.type)) {
                showFormError("Select a JPG, PNG or WebP image.");
                return;
            }

            if (profilePicture.size > MAX_PROFILE_SIZE) {
                showFormError("Profile picture must not exceed 2 MB.");
                return;
            }

            formData.append("ProfilePicture", profilePicture);
        }

        if (isEdit) {

            formData.append(
                "IsActive",
                String(document.getElementById("IsActive").checked)
            );

        } else {

            formData.append(
                "Password",
                document.getElementById("Password").value
            );
        }

        const button = document.getElementById("btnSaveUser");
        const spinner = document.getElementById("saveSpinner");
        const errorBox = document.getElementById("userFormError");

        button.disabled = true;
        spinner.classList.remove("d-none");

        errorBox.textContent = "";
        errorBox.classList.add("d-none");

        try {

            const url = isEdit
                ? `${API}/Update?id=${encodeURIComponent(id)}`
                : `${API}/Create`;

            await request(url, {
                method: isEdit ? "PUT" : "POST",
                body: formData
            });

            modal("userModal")?.hide();

            await loadUsers();

        } catch (error) {

            if (error instanceof ApiError) {
                showValidationErrors(error);
            } else {
                showFormError(error.message);
            }

        } finally {

            button.disabled = false;
            spinner.classList.add("d-none");
        }
    }
    function showFormError(message) {

        const errorBox = document.getElementById("userFormError");

        if (!errorBox) return;

        errorBox.textContent = message;

        errorBox.classList.remove("d-none");
    }

    function openDeleteModal(id) {

        selectedDeleteId = id;

        modal("deleteUserModal")?.show();
    }

    async function confirmDelete() {

        if (!selectedDeleteId) return;

        const button = document.getElementById("btnConfirmDelete");

        button.disabled = true;

        try {

            await request(
                `${API}/Delete?id=${encodeURIComponent(selectedDeleteId)}`,
                {
                    method: "DELETE"
                }
            );

            modal("deleteUserModal")?.hide();

            selectedDeleteId = null;

            await loadUsers();

        } catch (error) {

            alert(error.message);

        } finally {

            button.disabled = false;
        }
    }

    document.addEventListener("click", function (event) {

        const page = currentPage();

        if (!page || !page.contains(event.target)) return;

        if (event.target.closest("#btnAddUser")) {

            openAddModal();

            return;
        }

        const actionButton = event.target.closest("[data-action]");

        if (!actionButton) return;

        const id = actionButton.dataset.id;

        switch (actionButton.dataset.action) {

            case "view":
                openViewModal(id);
                break;

            case "edit":
                openEditModal(id);
                break;

            case "delete":
                openDeleteModal(id);
                break;
        }
    });

    document.addEventListener("submit", function (event) {

        if (event.target.id !== "userForm") return;

        console.log("1. User form submitted");

        saveUser(event);
    });

    document.addEventListener("click", function (event) {

        const button = event.target.closest("[data-toggle-password]");

        if (!button) return;

        const input = document.getElementById(
            button.dataset.togglePassword
        );

        if (!input) return;

        const isPassword = input.type === "password";

        input.type = isPassword ? "text" : "password";

        button.innerHTML = isPassword
            ? '<i class="bi bi-eye-slash"></i>'
            : '<i class="bi bi-eye"></i>';

        button.setAttribute(
            "aria-label",
            isPassword ? "Hide password" : "Show password"
        );
    });

    document.addEventListener("input", function (event) {

        if (event.target.closest("#userForm")) {

            const input = event.target;

            input.classList.remove("is-invalid");

            const feedback = document.getElementById(`${input.id}-error`);

            if (feedback) {
                feedback.textContent = "";
            }
        }


        if (
            event.target.id === "FirstName" ||
            event.target.id === "LastName"
        ) {

            updateProfilePreview();

            return;
        }

        if (event.target.id === "userSearch") {

            const term = event.target.value.toLowerCase();

            const filtered = users.filter(user => {

                const searchable = [

                    get(user, "EmployeeCode"),
                    get(user, "Username"),
                    get(user, "FirstName"),
                    get(user, "LastName"),
                    get(user, "Email"),
                    get(user, "Mobile")

                ].join(" ").toLowerCase();

                return searchable.includes(term);
            });

            renderUsers(filtered);
        }
    });

    document.addEventListener("change", function (event) {

        if (event.target.id !== "ProfilePicture") return;

        const input = event.target;

        const file = input.files?.[0];

        releaseProfilePreviewUrl();

        if (!file) {

            updateProfilePreview(currentAvatarUrl);

            return;
        }

        if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {

            input.value = "";

            showFormError("Select a JPG, PNG or WebP image.");

            updateProfilePreview(currentAvatarUrl);

            return;
        }

        if (file.size > MAX_PROFILE_SIZE) {

            input.value = "";

            showFormError("Profile picture must not exceed 2 MB.");

            updateProfilePreview(currentAvatarUrl);

            return;
        }

        const errorBox = document.getElementById("userFormError");

        errorBox.textContent = "";

        errorBox.classList.add("d-none");

        profilePreviewUrl = URL.createObjectURL(file);

        updateProfilePreview(profilePreviewUrl);
    });

    document.addEventListener("click", function (event) {

        if (event.target.closest("#btnConfirmDelete")) {

            confirmDelete();
        }
    });

    document.addEventListener("hidden.bs.modal", function (event) {

        if (event.target.id === "userModal") {

            releaseProfilePreviewUrl();
        }
    });

    const observer = new MutationObserver(() => {

        const page = currentPage();

        if (page && page.dataset.loaded !== "true") {

            page.dataset.loaded = "true";

            loadUsers();
        }
    });

    observer.observe(
        document.getElementById("main-content") || document.body,
        {
            childList: true,
            subtree: true
        }
    );

    if (currentPage()) {

        currentPage().dataset.loaded = "true";

        loadUsers();
    }

})();
