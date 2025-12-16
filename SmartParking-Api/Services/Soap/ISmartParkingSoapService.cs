using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using SmartParking_Api.Dtos;
using SmartParking_Api.Models;

namespace SmartParking_Api.Services.Soap;

[ServiceContract]
public interface ISmartParkingSoapService
{
    // Resumo de um parque +  meteorologia
    [OperationContract]
    Task<ParqueResumoDto?> GetParqueResumoAsync(int parqueId);

    // Lugares de um parque
    [OperationContract]
    Task<List<Lugar>> GetLugaresPorParqueAsync(int parqueId);
}