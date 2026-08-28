using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents a vendor quotation / procurement purchase order business object with procurement domain behaviors.
    /// </summary>
    [Table("purchases")]
    public class Purchase
    {
        /// <summary>
        /// Unique purchase record identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the original approved approval request.
        /// </summary>
        public int ApprovalRequestId { get; set; }

        /// <summary>
        /// Name of the approved item / product.
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Product Category.
        /// </summary>
        public string Category { get; set; } = "Hardware & Devices";

        /// <summary>
        /// Quantity approved for purchase.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Estimated amount from initial employee request.
        /// </summary>
        public decimal? EstimatedAmount { get; set; }

        /// <summary>
        /// Name of employee who raised the approved request.
        /// </summary>
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// Email of employee who raised the approved request.
        /// </summary>
        public string EmployeeEmail { get; set; } = string.Empty;

        /// <summary>
        /// Department name of the requesting employee.
        /// </summary>
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Name of the selected supplier / vendor.
        /// </summary>
        public string VendorName { get; set; } = string.Empty;

        /// <summary>
        /// Vendor contact phone or representative name.
        /// </summary>
        public string? VendorContact { get; set; }

        /// <summary>
        /// Vendor official email address.
        /// </summary>
        public string? VendorEmail { get; set; }

        /// <summary>
        /// Quotation reference number or proposal ID provided by vendor.
        /// </summary>
        public string? QuotationNumber { get; set; }

        /// <summary>
        /// Final quotation amount offered by the vendor.
        /// </summary>
        public decimal QuotationAmount { get; set; } = 0.00m;

        /// <summary>
        /// Date when the quotation was received / approved.
        /// </summary>
        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Expected delivery timeline (e.g. "3-5 Business Days", "2026-09-15").
        /// </summary>
        public string? DeliveryTimeline { get; set; }

        /// <summary>
        /// Payment terms (e.g. "Net 30 Days", "100% Advance", "50% Advance, 50% Delivery").
        /// </summary>
        public string? PaymentTerms { get; set; }

        /// <summary>
        /// Procurement notes, warranty terms, or remarks.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Procurement status: Quotation Received, PO Issued, In Procurement, Delivered, Completed, Cancelled.
        /// </summary>
        public string Status { get; set; } = "Quotation Received";

        /// <summary>
        /// ID of user (Manager / HR / Admin) who created the vendor purchase record.
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Name of user who created the vendor purchase record.
        /// </summary>
        public string? CreatedByName { get; set; }

        /// <summary>
        /// Record creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Record last modification timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag (1 = Active, 0 = Deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Purchase record linked to an approved employee request.
        /// </summary>
        public static Purchase CreateFromApproval(
            int approvalRequestId,
            string itemName,
            string? category,
            int quantity,
            decimal? estimatedAmount,
            string employeeName,
            string employeeEmail,
            string? departmentName,
            string vendorName,
            string? vendorContact,
            string? vendorEmail,
            string? quotationNumber,
            decimal quotationAmount,
            DateTime? quotationDate,
            string? deliveryTimeline,
            string? paymentTerms,
            string? notes,
            string? status,
            int createdByUserId,
            string? createdByName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
                throw new ArgumentException("Vendor name is required.", nameof(vendorName));

            var now = DateTime.UtcNow;
            return new Purchase
            {
                ApprovalRequestId = approvalRequestId,
                ItemName = itemName.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "Hardware & Devices" : category.Trim(),
                Quantity = quantity > 0 ? quantity : 1,
                EstimatedAmount = estimatedAmount,
                EmployeeName = employeeName.Trim(),
                EmployeeEmail = employeeEmail.Trim(),
                DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? null : departmentName.Trim(),
                VendorName = vendorName.Trim(),
                VendorContact = vendorContact?.Trim(),
                VendorEmail = vendorEmail?.Trim(),
                QuotationNumber = quotationNumber?.Trim(),
                QuotationAmount = Math.Max(0, quotationAmount),
                QuotationDate = quotationDate ?? now,
                DeliveryTimeline = string.IsNullOrWhiteSpace(deliveryTimeline) ? "3-5 Business Days" : deliveryTimeline.Trim(),
                PaymentTerms = string.IsNullOrWhiteSpace(paymentTerms) ? "Net 30" : paymentTerms.Trim(),
                Notes = notes?.Trim(),
                Status = string.IsNullOrWhiteSpace(status) ? "Quotation Received" : status.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedByName = createdByName,
                CreatedAt = now,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Updates the vendor quotation parameters and terms.
        /// </summary>
        public void UpdateQuotation(
            string vendorName,
            string? vendorContact,
            string? vendorEmail,
            string? quotationNumber,
            decimal quotationAmount,
            DateTime? quotationDate,
            string? deliveryTimeline,
            string? paymentTerms,
            string? notes,
            string? status)
        {
            if (!string.IsNullOrWhiteSpace(vendorName)) VendorName = vendorName.Trim();
            VendorContact = vendorContact?.Trim();
            VendorEmail = vendorEmail?.Trim();
            QuotationNumber = quotationNumber?.Trim();
            QuotationAmount = Math.Max(0, quotationAmount);
            if (quotationDate.HasValue) QuotationDate = quotationDate.Value;
            if (!string.IsNullOrWhiteSpace(deliveryTimeline)) DeliveryTimeline = deliveryTimeline.Trim();
            if (!string.IsNullOrWhiteSpace(paymentTerms)) PaymentTerms = paymentTerms.Trim();
            Notes = notes?.Trim();
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Transitions the procurement status.
        /// </summary>
        public void UpdateStatus(string newStatus, string? notes = null)
        {
            if (!string.IsNullOrWhiteSpace(newStatus)) Status = newStatus.Trim();
            if (notes != null) Notes = notes.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the purchase record.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
