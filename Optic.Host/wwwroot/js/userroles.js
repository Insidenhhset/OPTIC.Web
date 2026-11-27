
(function () {
    "use strict";

    const API = "/IdentityRoles";
    const toast = document.getElementById("opticToast");
    let allRoles = [];
    let searchText = "";
    let statusFilter = "active";
    let selectedRoleId = null;
    let selectedRoleStatus = null;

    let toastTimer = null;

    async function loadRoles() {
        const response = await fetch(`${API}/GetAll`, {
            method: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        if (!response.ok) {
            throw new Error("Unable to refresh roles.");
        }

        allRoles = await response.json();

        renderRoles();
    }

    function renderRoles() {
        const tbody = document.getElementById("roleTableBody");

        if (!tbody) return;

        const filteredRoles = allRoles.filter(role => {
            const matchesStatus =
                statusFilter === "active"
                    ? role.isActive
                    : !role.isActive;

            const search = searchText.toLowerCase();

            const matchesSearch =
                String(role.roleCode || "").toLowerCase().includes(search) ||
                String(role.roleName || "").toLowerCase().includes(search) ||
                String(role.description || "").toLowerCase().includes(search);

            return matchesStatus && matchesSearch;
        });

        if (filteredRoles.length === 0) {
            tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    No roles found.
                </td>
            </tr>`;
            return;
        }

        tbody.innerHTML = filteredRoles.map(role => `
        <tr>
            <td class="fw-semibold">${escapeHtml(role.roleCode)}</td>
            <td>${escapeHtml(role.roleName)}</td>
            <td>${escapeHtml(role.description || "-")}</td>
            <td>
                ${role.isSystem
                ? '<span class="badge bg-info-subtle text-info">System</span>'
                : '<span class="badge bg-secondary-subtle text-secondary">Custom</span>'}
            </td>
            <td>
                ${role.isActive
                ? '<span class="badge bg-success-subtle text-success">Active</span>'
                : '<span class="badge bg-danger-subtle text-danger">Inactive</span>'}
            </td>
            <td class="text-end">
              <div class="d-flex justify-content-end gap-2">
                <button type="button"
                        class="btn btn-sm btn-outline-primary edit-role-btn"
                        data-id="${role.id}"
                        title="Edit Role">
                    <i class="bi bi-pencil-square"></i>
                </button>

                ${role.isActive
                ? `<button type="button"
                   class="btn btn-sm btn-outline-danger toggle-role-status-btn"
                   data-id="${role.id}"
                   data-status="false"
                   title="Deactivate Role">
                <i class="bi bi-person-x"></i>
           </button>`
                : `<button type="button"
                   class="btn btn-sm btn-outline-success toggle-role-status-btn"
                   data-id="${role.id}"
                   data-status="true"
                   title="Activate Role">
                <i class="bi bi-person-check"></i>
           </button>`
            }
</div>
            </td>
        </tr>
    `).join("");
    }

    function escapeHtml(value) {
        return String(value).replace(/[&<>"']/g, char => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        })[char]);
    }

    document.addEventListener("submit", async function (event) {

        const form = event.target;

        if (form.id !== "createRoleForm") return;

        event.preventDefault();

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const modalElement = document.getElementById("createRoleModal");
        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        const submitButton = form.querySelector('button[type="submit"]');

        const originalButtonHtml = submitButton.innerHTML;

        try {
            submitButton.disabled = true;
            submitButton.innerHTML =
                'Creating...<span class="spinner-border spinner-border-sm me-2"></span>';

            const response = await fetch(`${API}/Create`, {
                method: "POST",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json().catch(() => ({}));

            if (!response.ok) {
                if (response.status === 400 && result.errors) {
                    const messages = Object.values(result.errors)
                        .flat()
                        .join("\n");

                    throw new Error(
                        messages || "Please check the entered details."
                    );
                }

                throw new Error(
                    result.detail ||
                    result.title ||
                    result.message ||
                    "Unable to create role."
                );
            }

            modal.hide();
            form.reset();


            showMessage(result.message || "Role created successfully.", "success");

            await loadRoles();

        }
        catch (error) {
            showMessage(error.message, "error");
        }
        finally {
            submitButton.disabled = false;
            submitButton.innerHTML = originalButtonHtml;
        }
    });

    document.addEventListener("click", async function (event) {
        const button = event.target.closest(".edit-role-btn");

        if (!button) return;

        const roleId = button.dataset.id;

        try {
            const response = await fetch(
                `${API}/GetById?id=${encodeURIComponent(roleId)}`,
                {
                    method: "GET",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                }
            );

            const role = await response.json();

            if (!response.ok) {
                throw new Error(role.message || "Unable to load role details.");
            }

            document.getElementById("editRoleId").value = role.id;
            document.getElementById("editRoleCode").value = role.roleCode;
            document.getElementById("editRoleName").value = role.roleName;
            document.getElementById("editRoleDescription").value =
                role.description || "";
            document.getElementById("editRoleType").innerHTML =
                role.isSystem
                    ? '<span class="badge bg-info-subtle text-info px-3 py-2">System Role</span>'
                    : '<span class="badge bg-secondary-subtle text-secondary px-3 py-2">Custom Role</span>';

            const modalElement = document.getElementById("editRoleModal");
            bootstrap.Modal.getOrCreateInstance(modalElement).show();
        }
        catch (error) {
            console.error("Load role error:", error);
            showMessage(error.message, "error");
        }
    });

    document.addEventListener("submit", async function (event) {

        const form = event.target;

        if (form.id !== "editRoleForm") return;

        event.preventDefault();

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const modalElement = document.getElementById("editRoleModal");
        const modal = bootstrap.Modal.getOrCreateInstance(modalElement);

        const submitButton = form.querySelector('button[type="submit"]');
        const originalButtonHtml = submitButton.innerHTML;

        try {
            submitButton.disabled = true;

            submitButton.innerHTML = `
            Updating...
            <span class="spinner-border spinner-border-sm ms-2"
                  role="status"
                  aria-hidden="true"></span>
        `;

            const response = await fetch(`${API}/Update`, {
                method: "POST",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json().catch(() => ({}));

            if (!response.ok) {

                if (response.status === 400 && result.errors) {
                    const messages = Object.values(result.errors)
                        .flat()
                        .join("\n");

                    throw new Error(
                        messages || "Please check the entered details."
                    );
                }

                throw new Error(
                    result.detail ||
                    result.title ||
                    result.message ||
                    "Unable to update role."
                );
            }

            modal.hide();
            form.reset();

            showMessage(result.message || "Role updated successfully.", "success");

            await loadRoles();

        }
        catch (error) {

            console.error("Update role error:", error);

            showMessage(error.message, "error");

        }
        finally {
            submitButton.disabled = false;
            submitButton.innerHTML = originalButtonHtml;
        }
    });

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

    document.addEventListener("input", function (event) {
        if (event.target.id !== "roleSearch") return;

        searchText = event.target.value.trim();
        renderRoles();
    });

    document.addEventListener("change", function (event) {
        if (event.target.id !== "roleStatusFilter") return;

        statusFilter = event.target.value;
        renderRoles();
    });


    document.addEventListener("click", function (event) {
        const button = event.target.closest(".toggle-role-status-btn");

        if (!button) return;

        selectedRoleId = Number(button.dataset.id);
        selectedRoleStatus = button.dataset.status === "true";

        const isActivating = selectedRoleStatus;

        const icon = document.getElementById("roleStatusIcon");
        const title = document.getElementById("roleStatusTitle");
        const message = document.getElementById("roleStatusMessage");
        const confirmButton = document.getElementById("btnConfirmRoleStatus");

        if (isActivating) {
            icon.innerHTML = '<i class="bi bi-check-circle text-success"></i>';
            title.textContent = "Activate Role?";
            message.textContent = "Are you sure you want to activate this role?";

            confirmButton.textContent = "Activate";
            confirmButton.className = "btn btn-success";
        } else {
            icon.innerHTML = '<i class="bi bi-exclamation-circle text-danger"></i>';
            title.textContent = "Deactivate Role?";
            message.textContent = "Are you sure you want to deactivate this role?";

            confirmButton.textContent = "Deactivate";
            confirmButton.className = "btn btn-danger";
        }

        bootstrap.Modal.getOrCreateInstance(
            document.getElementById("roleStatusModal")
        ).show();
    });

    document.addEventListener("click", async function (event) {
        if (event.target.id !== "btnConfirmRoleStatus") return;

        const button = event.target;

        if (!selectedRoleId || selectedRoleStatus === null) return;

        const formData = new FormData();
        formData.append("id", selectedRoleId);
        formData.append("isActive", selectedRoleStatus);

        button.disabled = true;
        button.textContent = selectedRoleStatus ? "Activating..." : "Deactivating...";

        try {
            const response = await fetch(`${API}/UpdateStatus`, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: formData
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(result.detail || result.Message || result.title || "Unable to update role status.");
            }

            bootstrap.Modal.getInstance(
                document.getElementById("roleStatusModal")
            ).hide();

            showMessage(result.message || result.Message, "success");

            selectedRoleId = null;
            selectedRoleStatus = null;

            await loadRoles();
        } catch (error) {
            console.error("Update role status error:", error);
            showMessage(error.message, "error");
        } finally {
            button.disabled = false;
            button.textContent = selectedRoleStatus === true ? "Activate" : "Deactivate";
        }
    });

    loadRoles().catch(error => {
        console.error("Load roles error:", error);
        showMessage(error.message, "error");
    });
})();