using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Core;
using ClosedXML.Excel;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using worklog_api.error;
using worklog_api.Model;
using worklog_api.Repository;
using worklog_api.Repository.implementation;

namespace worklog_api.Service
{
    public class MOLService : IMOLService
    {
        private readonly IMOLRepository _molRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;

        public MOLService(IMOLRepository molRepository, IStatusHistoryRepository statusHistoryRepository)
        {
            _molRepository = molRepository;
            _statusHistoryRepository = statusHistoryRepository;
        }

        public async Task<(IEnumerable<MOLModel> mols, int totalCount)> GetAllMOLs(int pageNumber, int pageSize, string sortBy, string sortDirection, DateTime? startDate, DateTime? endDate, string requestBy, string status)
        {
            return await _molRepository.GetAll(pageNumber, pageSize, sortBy, sortDirection, startDate, endDate, requestBy, status);
        }


        public async Task<MOLModel> GetMOLById(Guid id)
        {
            return await _molRepository.GetById(id);
        }

        public async Task CreateMOL(MOLModel mol)
        {
            Console.WriteLine("Create MOL");
            try
            {
                await _molRepository.Create(mol);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError(e.Message);
            }
        }

        public async Task UpdateMOL(MOLModel mol)
        {
            await _molRepository.Update(mol);
        }

        public async Task DeleteMOL(Guid id)
        {
            await _molRepository.Delete(id);
        }

        public async Task ApproveMOL(StatusHistoryModel status, UserModel user, int quantityApproved)
        {
            var mol = await _molRepository.GetById(status.MOLID);
            if (mol == null)
            {
                throw new NotFoundException("MOL Not Found");
            }

            if (user.role == "Group Leader" && mol.Status == "PENDING")
            {
                status.Status = "APPROVED_GROUP_LEADER";
                await _molRepository.UpdateApprovedQuantity(mol.ID, quantityApproved);

            } 
            else if (user.role == "Data Planner" && mol.Status == "APPROVED_GROUP_LEADER") 
            {
                status.Status = "APPROVED_DATA_PLANNER";
                await _molRepository.UpdateApprovedQuantity(mol.ID, quantityApproved);
            }
            else
            {
                throw new AuthorizationException("Invalid Role Or Status Already Updated");
            }

            try
            {
                await _statusHistoryRepository.Create(status);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError(e.Message);
            }
        }

        public async Task Reject(StatusHistoryModel status, UserModel user)
        {
            var mol = await _molRepository.GetById(status.MOLID);
            if (mol == null)
            {
                throw new NotFoundException("MOL Not Found");
            }

            if (mol.Status == "REJECTED")
            {
                throw new InternalServerError("MOL already rejected");
            }

            //if (user.role == "Group Leader" && mol.Status == "PENDING")
            //{
            //    status.Status = "REJECTED";
            //}
            //else
            //{
            //    throw new AuthorizationException("Invalid Role Or Status Already Updated");
            //}

            status.Status = "REJECTED";

            try
            {
                await _statusHistoryRepository.Create(status);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError(e.Message);
            }
        }

        public async Task<byte[]> ExportMolToExcel(string status)
        {
            // Fetch data from repository
            var mols = await _molRepository.GetMolByStatus("COMPLETED");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("MOL Data");

                // Define headers
                string[] headers = {
                    "ID", "Kode Number", "Tanggal", "Work Order", "Hour Meter",
                    "Kode Komponen", "Part Number", "Description", "Quantity", "Quantity Approved",
                    "Categories", "Remark", "Request By", "Status", "Version",
                    "Created By", "Updated By", "Created At", "Updated At"
                };

                // Create header row
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // Populate data rows
                for (int i = 0; i < mols.Count(); i++)
                {
                    var mol = mols.ElementAt(i);
                    worksheet.Cell(i + 2, 1).Value = mol.ID.ToString();
                    worksheet.Cell(i + 2, 2).Value = mol.KodeNumber;
                    worksheet.Cell(i + 2, 3).Value = mol.Tanggal.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 4).Value = mol.WorkOrder;
                    worksheet.Cell(i + 2, 5).Value = mol.HourMeter;
                    worksheet.Cell(i + 2, 6).Value = mol.KodeKomponen;
                    worksheet.Cell(i + 2, 7).Value = mol.PartNumber;
                    worksheet.Cell(i + 2, 8).Value = mol.Description;
                    worksheet.Cell(i + 2, 9).Value = mol.Quantity;
                    worksheet.Cell(i + 2, 10).Value = mol.QuantityApproved;
                    worksheet.Cell(i + 2, 11).Value = mol.Categories;
                    worksheet.Cell(i + 2, 12).Value = mol.Remark;
                    worksheet.Cell(i + 2, 13).Value = mol.RequestBy;
                    worksheet.Cell(i + 2, 14).Value = mol.Status;
                    worksheet.Cell(i + 2, 15).Value = mol.Version;
                    worksheet.Cell(i + 2, 16).Value = mol.CreatedBy;
                    worksheet.Cell(i + 2, 17).Value = mol.UpdatedBy;
                    worksheet.Cell(i + 2, 18).Value = mol.CreatedAt?.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 19).Value = mol.UpdatedAt?.ToString("yyyy-MM-dd");
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Save workbook to memory stream
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
