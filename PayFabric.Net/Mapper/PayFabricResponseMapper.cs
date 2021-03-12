using PayFabric.Net.Models;
using SSCo.PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PayFabric.Net.Mapper
{
    public class PayFabricResponseMapper
    {
        public void MaptoServiceNetResponse(ServiceNetResponse serviceNetResponse, PayFabricResponse payFabricResponse)
        {
            if (payFabricResponse != null)
            {
                serviceNetResponse.Success = payFabricResponse.Status.ToLower() == "approved" ? true : false;
                serviceNetResponse.Transaction = new TransactionResult
                {
                    AuthorizationCode = payFabricResponse.AuthCode,
                    AVSAddressResult = payFabricResponse.AVSAddressResponse,
                    AVSZipResult = payFabricResponse.AVSZipResponse,
                    CVV2Result = payFabricResponse.CVV2Response,
                    ResultCode = payFabricResponse.ResultCode,
                    Status = getStatus(payFabricResponse.Status),
                    TransactionKey = payFabricResponse.TrxKey,
                    TransactionDate = payFabricResponse.TrxDate?.ToLongDateString(),
                    StatusCode = payFabricResponse.Status,
                    ResultMessage = payFabricResponse.Message,
                    OriginationId = payFabricResponse.OriginationID,
                    ExtentionInformation = new Dictionary<string, object>() {
                            {"PayFabricErrorCode", payFabricResponse.PayFabricErrorCode }
                        }
                };
                serviceNetResponse.ResponseMessage = payFabricResponse.Message;
            }
        }
        private TransactionStatus getStatus(string ststus)
        {
            switch (ststus.ToLower())
            {
                case "approved":
                    return TransactionStatus.Approved;

                case "failure":
                    return TransactionStatus.Failure;
                case "declined":
                    return TransactionStatus.Declined;
                case "denied":
                    return TransactionStatus.Denied;
                case "avsfailure":
                    return TransactionStatus.AVSFailure;
                case "none":
                    return TransactionStatus.Unknown;
                default:
                    return TransactionStatus.Unknown;

            }
        }
    }
}
