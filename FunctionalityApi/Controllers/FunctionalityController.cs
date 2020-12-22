using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FunctionalityApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FunctionalityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionalityController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public FunctionalityController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var cacheKey = "_values";
            var cacheKeyWithoutExpiration = "_valuesWithoutExpiration";
            try
            {
                if (!_cache.TryGetValue(cacheKey, out List<FunctionalityDetail> values)) // Busca la clave de cache.
                {
                    var jsonString = System.IO.File.ReadAllText("data.json");

                    values = JsonSerializer.Deserialize<List<FunctionalityDetail>>(jsonString, new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });

                    _cache.Set(cacheKey, values, new MemoryCacheEntryOptions() // Opciones de cache.
                    {
                        // Podemos establecer la caducidad real de la entrada de caché, siempre (AbsoluteExpiration > SlidingExpiration)
                        AbsoluteExpiration = DateTime.Now.AddMinutes(10),
                        // Establece la prioridad de mantener la entrada de la caché en la caché
                        Priority = CacheItemPriority.Normal,
                        // Permite establecer el tamaño de esta caché en particular, para que no comience a consumir los recursos del servidor.
                        Size = 1024,
                        // Mantener en caché durante este tiempo, restablecer el tiempo si se accede.
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    });

                    _cache.Set(cacheKeyWithoutExpiration, values, new MemoryCacheEntryOptions()
                    {
                        Priority = CacheItemPriority.Normal,
                        Size = 1024,
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    });
                }

                return Ok(values);
            }
            catch (Exception)
            {
                if (_cache.TryGetValue(cacheKeyWithoutExpiration, out List<FunctionalityDetail> values))
                {
                    return Ok(values);
                };
            }

            return BadRequest();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var jsonString = System.IO.File.ReadAllText("data.json");
            var jsonModelList = JsonSerializer.Deserialize<List<FunctionalityDetail>>(jsonString, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var entity = jsonModelList.Where(x => x.Id == id).FirstOrDefault();

            if (entity == null) return BadRequest();

            return Ok(entity);
        }

        [HttpPost]
        public IActionResult Create(FunctionalityDetail model)
        {
            if (ModelState.IsValid)
            {
                var jsonString = System.IO.File.ReadAllText("data.json");
                var jsonModelList = JsonSerializer.Deserialize<List<FunctionalityDetail>>(jsonString, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                jsonModelList.Add(model);

                var jsonModel = JsonSerializer.Serialize(jsonModelList, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                System.IO.File.WriteAllText("data.json", jsonModel);

                return Ok(model.Id);
            };

            return BadRequest();
        }

        [HttpPut("{id}")]
        public IActionResult Edit(int id, FunctionalityDetail model)
        {
            if (ModelState.IsValid)
            {
                var jsonString = System.IO.File.ReadAllText("data.json");
                var jsonModelList = JsonSerializer.Deserialize<List<FunctionalityDetail>>(jsonString, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                var replaceEntity = jsonModelList.Where(x => x.Id == id).FirstOrDefault();

                if (replaceEntity == null) return BadRequest();

                jsonModelList.Remove(replaceEntity);
                jsonModelList.Add(model);

                var jsonModel = JsonSerializer.Serialize(jsonModelList, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                System.IO.File.WriteAllText("data.json", jsonModel);

                return Ok(model.Id);
            }

            return BadRequest();
        }

    }
}
