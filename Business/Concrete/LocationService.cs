using Business.Abstract;
using Core.Aspects.Autofac.Cache;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class LocationService : ILocationService
    {
        IRestClient restClient;
        ILocationRepository _locationRepository;



        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
            restClient = new RestClient();
        }

        [CacheRemoveAspect("ILocationService.GetLocationsByIpAddress")]
        public IResult Add(Location location)
        {

            _locationRepository.Add(location);

            return new SuccessResult("Location added");
        }

        [CacheAspect]
        public IDataResult<Location> GetLocationsByIpAddress(string ipAddress)
        {
            bool isAddressExists = _locationRepository.GetAll(i => i.IpAddress == ipAddress).Any();

            if (!isAddressExists )
            {

                var response = GetResponse(ipAddress);
                 

                if (response.IsSuccessful)
                {

                    JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);

                    Location location = new Location
                    {
                        IpAddress = jsonResponse["ip"]?.Value<string>(),
                        Continent = jsonResponse["continent_name"]?.Value<string>(),
                        Country = jsonResponse["country_name"]?.Value<string>(),
                        City = jsonResponse["city"]?.Value<string>(),
                        Latitude = jsonResponse["latitude"]?.Value<string>(),
                        Longitude = jsonResponse["longitude"]?.Value<string>(),
                        Type = jsonResponse["type"]?.Value<string>(),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    Add(location);


                    return new SuccessDataResult<Location>(location);
                }

            }

            return new SuccessDataResult<Location>(_locationRepository.Get(l => l.IpAddress == ipAddress));
        }

        [CacheRemoveAspect("ILocationService.GetLocationsByIpAddress")]
        public IResult Update(Location location)
        {
            bool existAddress = _locationRepository.GetAll(i => i.IpAddress == location.IpAddress).Any();

            var response = GetResponse(location.IpAddress);

            if (response.IsSuccessful)
            {
                JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);

                Location apiLocation = new Location
                {
                    IpAddress = jsonResponse["ip"]?.Value<string>(),
                    Continent = jsonResponse["continent_name"]?.Value<string>(),
                    Country = jsonResponse["country_name"]?.Value<string>(),
                    City = jsonResponse["city"]?.Value<string>(),
                    Latitude = jsonResponse["latitude"]?.Value<string>(),
                    Longitude = jsonResponse["longitude"]?.Value<string>(),
                    Type = jsonResponse["type"]?.Value<string>(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                if(!location.Equals(apiLocation))
                {

                    location.IpAddress = jsonResponse["ip"]?.Value<string>();
                    location.Continent = jsonResponse["continent_name"]?.Value<string>();
                    location.Country = jsonResponse["country_name"]?.Value<string>();
                    location.City = jsonResponse["city"]?.Value<string>();
                    location.Latitude = jsonResponse["latitude"]?.Value<string>();
                    location.Longitude = jsonResponse["longitude"]?.Value<string>();
                    location.Type = jsonResponse["type"]?.Value<string>();
                    location.UpdatedDate = DateTime.Now;
                }
            }

            return new SuccessResult("Data updated");
        }

        private RestResponse GetResponse(string ipAddress)
        {
            string baseUrl = "https://api.apilayer.com/ip_to_location/";
            var client = new RestClient($"{baseUrl}{ipAddress}", configureSerialization: s => s.UseDefaultSerializers());
            var request = new RestRequest().AddHeader("apikey", "46GQXdCOWn5exIZts0h4KjUvHtGEnxfJ");
            var response = client.Get(request);

            return response;
        }
    }
}
