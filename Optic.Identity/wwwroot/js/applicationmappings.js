
window.initializeApplicationMappings = function () {
    "use strict";

    const API = "/IdentityRoles";

    const applicationSelect = document.getElementById("applicationSelect");
    const applicationInfo = document.getElementById("selectedApplicationInfo");
    const tableBody = document.getElementById("applicationRoleMappingTableBody");
    const mappedRoleCount = document.getElementById("mappedRoleCount");
    const searchInput = document.getElementById("mappingSearch");
    const filterSelect = document.getElementById("mappingStatusFilter");

    if (
        !applicationSelect ||
        !applicationInfo ||
        !tableBody ||
        !mappedRoleCount ||
        !searchInput ||
        !filterSelect
    ) {
        return;
    }

    if (applicationSelect.dataset.initialized === "true") {
        return;
    }

    applicationSelect.dataset.initialized = "true";

    let applications = [];
    let roleMappings = [];
    let selectedApplicationId = null;
    let searchText = "";
    let statusFilter = "all";

    async function loadApplications() {
        try {
            const response = await fetch(`${API}/GetApplications`, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.detail ||
                    result.Message ||
                    "Unable to load applications."
                );
            }

            applications = result;

            applicationSelect.innerHTML =
                '<option value="">Select Application</option>';

            applications.forEach(app => {
                const option = document.createElement("option");
                option.value = app.id;
                option.textContent =
                    `${app.applicationCode} - ${app.applicationName}`;

                applicationSelect.appendChild(option);
            });

        } catch (error) {
            console.error("Load applications error:", error);

            applicationSelect.innerHTML =
                '<option value="">Unable to load applications</option>';
        }
    }

    async function loadRoleMappings(applicationId) {
        try {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center py-4">
                       <span class="spinner-border spinner-border-sm"></span> Loading roles...
                    </td>
                </tr>`;

            const response = await fetch(
                `${API}/GetApplicationRoleMappings?applicationId=${encodeURIComponent(applicationId)}`,
                {
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                }
            );

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.detail ||
                    result.Message ||
                    "Unable to load role mappings."
                );
            }

            roleMappings = result;

            renderRoleMappings();

            const application = applications.find(
                app => Number(app.id) === Number(applicationId)
            );

            if (application) {
                applicationInfo.textContent =
                    `${application.applicationName} (${application.applicationCode})`;
            }

        } catch (error) {
            console.error("Load role mappings error:", error);

            tableBody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-danger py-4">
                        ${escapeHtml(error.message)}
                    </td>
                </tr>`;
        }
    }

    function renderRoleMappings() {
        const search = searchText.toLowerCase();

        const filteredRoles = roleMappings.filter(role => {
            const matchesSearch =
                String(role.roleCode || "").toLowerCase().includes(search) ||
                String(role.roleName || "").toLowerCase().includes(search);

            let matchesStatus = true;

            switch (statusFilter) {
                case "mapped":
                    matchesStatus = role.isMapped === true;
                    break;

                case "unmapped":
                    matchesStatus = role.mappingId === null;
                    break;

                case "inactive":
                    matchesStatus =
                        role.mappingId !== null &&
                        role.mappingIsActive === false;
                    break;
            }

            return matchesSearch && matchesStatus;
        });

        const activeMappings = roleMappings.filter(
            role => role.isMapped === true
        ).length;

        mappedRoleCount.textContent = `${activeMappings} Mapped`;

        if (filteredRoles.length === 0) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted py-4">
                        No roles found.
                    </td>
                </tr>`;
            return;
        }

        tableBody.innerHTML = filteredRoles.map(role => {
            let mappingStatus = "";

            if (role.mappingId === null) {
                mappingStatus =
                    '<span class="badge bg-secondary">Not Mapped</span>';
            } else if (role.mappingIsActive) {
                mappingStatus =
                    '<span class="badge bg-success">Mapped</span>';
            } else {
                mappingStatus =
                    '<span class="badge bg-danger">Inactive</span>';
            }

            return `
                <tr>
                    <td>${escapeHtml(role.roleCode)}</td>
                    <td>${escapeHtml(role.roleName)}</td>
                    <td>${escapeHtml(role.roleDescription || "-")}</td>
                    <td>
                        ${role.roleIsActive
                    ? '<span class="badge bg-success">Active</span>'
                    : '<span class="badge bg-secondary">Inactive</span>'}
                    </td>
                    <td>${mappingStatus}</td>
                 <td class="text-end">
    <div class="d-flex justify-content-end gap-2">

        ${role.mappingId === null
                    ? `<button type="button"
                       class="btn btn-sm btn-outline-primary mapping-action-btn"
                       data-action="map"
                       data-role-id="${role.roleId}"
                       title="Map Role">
                    <i class="bi bi-link-45deg"></i>
               </button>`
                    : `
                ${role.mappingIsActive
                        ? `<button type="button"
                               class="btn btn-sm btn-outline-warning mapping-action-btn"
                               data-action="deactivate"
                               data-role-id="${role.roleId}"
                               data-mapping-id="${role.mappingId}"
                               title="Deactivate Mapping">
                            <i class="bi bi-pause-circle"></i>
                       </button>`
                        : `<button type="button"
                               class="btn btn-sm btn-outline-success mapping-action-btn"
                               data-action="activate"
                               data-role-id="${role.roleId}"
                               data-mapping-id="${role.mappingId}"
                               title="Activate Mapping">
                            <i class="bi bi-check-circle"></i>
                       </button>`
                    }

                <button type="button"
                        class="btn btn-sm btn-outline-danger mapping-action-btn"
                        data-action="unmap"
                        data-role-id="${role.roleId}"
                        data-mapping-id="${role.mappingId}"
                        title="Unmap Role">
                    <i class="bi bi-link-45deg"></i>
                </button>
            `
        }

    </div>
</td>
                </tr>`;
        }).join("");
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

    applicationSelect.addEventListener("change", function () {
        selectedApplicationId = this.value;

        searchText = "";
        statusFilter = "all";

        searchInput.value = "";
        filterSelect.value = "all";

        if (selectedApplicationId) {
            loadRoleMappings(selectedApplicationId);
        } else {
            roleMappings = [];

            applicationInfo.textContent =
                "Select an application to manage its role mappings.";

            tableBody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted py-4">
                        Select an application to view roles.
                    </td>
                </tr>`;

            mappedRoleCount.textContent = "0 Mapped";
        }
    });

    searchInput.addEventListener("input", function () {
        searchText = this.value.trim();
        renderRoleMappings();
    });

    filterSelect.addEventListener("change", function () {
        statusFilter = this.value;
        renderRoleMappings();
    });

    const mappingModalElement = document.getElementById("mappingConfirmationModal");

    if (!mappingModalElement || !window.bootstrap?.Modal) {
        console.error("Mapping confirmation modal or Bootstrap Modal is unavailable.");
        return;
    }

    const mappingModal = bootstrap.Modal.getOrCreateInstance(mappingModalElement);


    const confirmationTitle = document.getElementById("mappingConfirmationModalLabel");
    const confirmationMessage = document.getElementById("mappingConfirmationMessage");
    const confirmationDescription = document.getElementById("mappingConfirmationDescription");
    const confirmationIcon = document.getElementById("mappingConfirmationIcon");
    const confirmButton = document.getElementById("confirmMappingActionBtn");

    let pendingMappingAction = null;

    const actionConfig = {
        map: {
            title: "Map Role",
            message: "Are you sure you want to map this role?",
            description: "This role will become available in the selected application.",
            icon: "bi-link-45deg",
            color: "text-primary",
            button: "btn-primary",
            buttonText: "Map Role"
        },
        deactivate: {
            title: "Deactivate Mapping",
            message: "Are you sure you want to deactivate this mapping?",
            description: "The role will remain mapped but will become inactive.",
            icon: "bi-pause-circle",
            color: "text-warning",
            button: "btn-warning",
            buttonText: "Deactivate"
        },
        activate: {
            title: "Activate Mapping",
            message: "Are you sure you want to activate this mapping?",
            description: "The role will become active in the selected application.",
            icon: "bi-check-circle",
            color: "text-success",
            button: "btn-success",
            buttonText: "Activate"
        },
        unmap: {
            title: "Unmap Role",
            message: "Are you sure you want to permanently unmap this role?",
            description: "This will remove the application-role association.",
            icon: "bi-unlink",
            color: "text-danger",
            button: "btn-danger",
            buttonText: "Unmap"
        }
    };

    tableBody.addEventListener("click", function (event) {
        const actionButton = event.target.closest(".mapping-action-btn");

        if (!actionButton) return;

        const action = actionButton.dataset.action;
        const config = actionConfig[action];

        if (!config) return;

        pendingMappingAction = {
            action,
            roleId: Number(actionButton.dataset.roleId),
            mappingId: actionButton.dataset.mappingId
                ? Number(actionButton.dataset.mappingId)
                : null
        };

        confirmationTitle.textContent = config.title;
        confirmationMessage.textContent = config.message;

        confirmationIcon.className = `fs-3 ${config.color}`;
        confirmationIcon.innerHTML = `<i class="bi ${config.icon}"></i>`;

        confirmButton.className = `btn ${config.button}`;
        confirmButton.textContent = config.buttonText;
        confirmButton.disabled = false;

        mappingModal.show();
    });


    confirmButton.addEventListener("click", async function () {
        if (!pendingMappingAction) return;

        const { action, roleId, mappingId } = pendingMappingAction;

        const config = actionConfig[action];

        if (!config) return;

        if (!selectedApplicationId || !roleId) {
            if (action === "map") return;
        }

        if (action !== "map" && !mappingId) {
            confirmationMessage.textContent = "Invalid mapping selected.";
            return;
        }

        let url = "";
        const params = new URLSearchParams();

        if (action === "map") {
            url = `${API}/CreateApplicationRoleMapping`;
            params.append("applicationId", selectedApplicationId);
            params.append("roleId", roleId);
        }
        else if (action === "activate") {
            url = `${API}/ActivateApplicationRoleMapping`;
            params.append("mappingId", mappingId);
        }
        else if (action === "deactivate") {
            url = `${API}/DeactivateApplicationRoleMapping`;
            params.append("mappingId", mappingId);
        }
        else if (action === "unmap") {
            url = `${API}/DeleteApplicationRoleMapping`;
            params.append("mappingId", mappingId);
        }
        else {
            return;
        }

        try {
            confirmButton.disabled = true;
            confirmButton.innerHTML =
                '<span class="spinner-border spinner-border-sm me-1"></span> Processing...';

            const response = await fetch(`${url}?${params.toString()}`, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(
                    result.message ||
                    result.detail ||
                    "Unable to complete the mapping operation."
                );
            }

            mappingModal.hide();
            pendingMappingAction = null;

            await loadRoleMappings(selectedApplicationId);

        } catch (error) {
            console.error("Mapping action error:", error);
            confirmationMessage.textContent = error.message;

        } finally {
            confirmButton.disabled = false;
            confirmButton.className = `btn ${config.button}`;
            confirmButton.textContent = config.buttonText;
        }
    });


    mappingModalElement.addEventListener("hidden.bs.modal", function () {
        pendingMappingAction = null;
    });


    loadApplications();
};

if (document.readyState === "loading") {
    document.addEventListener(
        "DOMContentLoaded",
        window.initializeApplicationMappings
    );
} else {
    window.initializeApplicationMappings();
}
