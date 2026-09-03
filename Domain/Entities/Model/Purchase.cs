using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities.Model
{
    [Table("purchases")]
    public class Purchase
    {
        public int Id { get; set; }

        public int ApprovalRequestId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string Category { get; set; } = "Hardware & Devices";

        public int Quantity { get; set; } = 1;

        public decimal? EstimatedAmount { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeEmail { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }

        public string VendorName { get; set; } = string.Empty;

        public string? VendorContact { get; set; }

        public string? VendorEmail { get; set; }

        public string? QuotationNumber { get; set; }

        public decimal QuotationAmount { get; set; } = 0.00m;

        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

        public string? DeliveryTimeline { get; set; }

        public string? PaymentTerms { get; set; }

        public string? Notes { get; set; }

        public string Status { get; set; } = "Quotation Received";

        public int CreatedByUserId { get; set; }

        public string? CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

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

        public void UpdateStatus(string newStatus, string? notes = null)
        {
            if (!string.IsNullOrWhiteSpace(newStatus)) Status = newStatus.Trim();
            if (notes != null) Notes = notes.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
