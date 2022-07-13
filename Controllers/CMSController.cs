using Dapper;
using Microsoft.AspNetCore.Mvc;
using Outbound_Api.Data;
using Outbound_Api.Services;
using Outbound_Api.Models;
using System.Data;

namespace Outbound_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CMSController : ControllerBase
    {
        private readonly ILogger<CMSController> _logger;
        private readonly AppDbContext _db;
        private readonly IDapper dapper;

        public CMSController(ILogger<CMSController> logger, AppDbContext db, IDapper dapper)
        {
            _logger = logger;
            _db = db;
            this.dapper = dapper;
        }

        [HttpGet("CheckPickingNo/{pickingNo}", Name = "CheckPickingNo")]
        public Response CheckPickingNo(string pickingNo)
        {
            string sql = "SELECT * FROM TL_SAP_DOC WHERE Ref1='" + pickingNo + "'";
            var results = dapper.executeQuerry(sql);

            var response = new Response();

            if (results.Count > 0)
            {
                response.response ="found";
            }

            response.response = "Record Not Found";

            return response;
        }

        [HttpGet("GetTyreData/{tyreBarcode}/{picking}")]
        public TyreData GetTyreData(string tyreBarcode, string picking)
        {
            string docType = "";
            string matCode = "";
            string message = "";
            string ospNo = "";
            string IpCode = "";
            string maktx = "";
            int quantity=0;
            int completedNo = 0;

            var dbparams = new DynamicParameters();

            dbparams.Add("@SerialNumber", tyreBarcode, DbType.String, ParameterDirection.Input);
            dbparams.Add("@Ref2", picking, DbType.String, ParameterDirection.Input);

            dbparams.Add("@DocType", docType, DbType.String, ParameterDirection.Output);
            dbparams.Add("@DocNumber", ospNo, DbType.String, ParameterDirection.Output);
            dbparams.Add("@IpCode", IpCode, DbType.String, ParameterDirection.Output);
            dbparams.Add("@Stok", quantity, DbType.Int32, ParameterDirection.Output);
            dbparams.Add("@MatCode", matCode, DbType.String, ParameterDirection.Output);
            dbparams.Add("@MAT_DESC", maktx, DbType.String, ParameterDirection.Output);
            dbparams.Add("@Mesaj", message, DbType.String, ParameterDirection.Output);
            dbparams.Add("@Adet", completedNo, DbType.Int32, ParameterDirection.Output);

            dbparams.Add("@return_value", null, DbType.Int32, ParameterDirection.ReturnValue);

            dbparams = dapper.Execute($"[dbo].[csp_TL_OUT_SP0002]", dbparams, commandType: CommandType.StoredProcedure);

            var tyre = new TyreData();

            tyre.return_value = dbparams.Get<Int32>("@return_value");
            tyre.Mesaj = dbparams.Get<string>("@Mesaj");
            tyre.Adet = dbparams.Get<dynamic>("@Adet");
            tyre.Stok = dbparams.Get<dynamic>("@Stok");
            tyre.DocNumber = dbparams.Get<string>("@DocNumber");
            tyre.IpCode = dbparams.Get<string>("@IpCode");
            tyre.MAT_DESC = dbparams.Get<string>("@MAT_DESC");

            return tyre;
        }

        [HttpPut("updatedeliveryip/{control}/{picking}/{ospNo}/{IpCode}")]
        public Response updatedeliveryip(bool control, string ospNo, string picking, string IpCode)
        {
            string status;

            if (control == true)
                status = "2";
            else
                status = "3";

            string querry = "update TL_SAP_DOC set STATUS='" + status + "' OUTPUT INSERTED.IpCode where  DocType = 'OD' and  DocNumber = '" + ospNo + "' and Ref2 = '" + picking + "' and IpCode like '" + IpCode + "%' ";

            var results = dapper.executeQuerry(querry);

            var response = new Response();

            if (results.Count > 0)
            {
                string sql = "SELECT IpCode FROM TL_SAP_DOC_INB_VIEW  with(nolock) WHERE (Ref2 like '" + picking + "%') and  DocType = 'OD' and  DocNumber like '" + ospNo + "%' and IpCode like '" + IpCode + "%' and (STATUS ='0' or STATUS ='1') ";

                results = dapper.executeQuerry(sql);

                if (results.Count > 0)
                {
                    response.response = "IPCODE OFF! ...";
                }
                else
                {
                    response.response = "Tires Taught !.";
                }

                return response;
            }

            response.response = "None";
            return response;
        }

        [HttpGet("getSerialNoByPalletNo/{palletNo}")]
        public List<Pallet> getSerialNoByPalletNo(String palletNo)
        {
            string sql = "SELECT SerialNumber FROM TL_INB_TYRES  with(nolock) WHERE (PalletNumber like '" + palletNo + "%') ";

            var results = dapper.executeQuerry(sql);

            List<Pallet> list = new List<Pallet>();

            for (int i = 0; i < results.Count; i++)
            {
                //9MSECU
                var pallet = new Pallet();
                var data = (IDictionary<string, object>)results[i];
                pallet.SerialNumber = data["SerialNumber"].ToString();
                list.Add(pallet);
            }

            return list;
        }
    }
}
