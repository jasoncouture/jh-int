using BillManager.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BillManager.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonRepository _personRepository;
        public PersonController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<PersonModel>))]
        public async Task<IActionResult> GetAsync(int? count = null, int? offset = null, CancellationToken cancellationToken = default)
        {
            List<PersonModel> people = new();
            await foreach(var person in _personRepository.GetPeople(count, offset).WithCancellation(cancellationToken))
            {
                people.Add(person);
            }
            return Ok(people);
        }

        [HttpGet("{id}")]
        [Produces(typeof(PersonModel))]
        public async Task<IActionResult> GetAsync(long id, CancellationToken cancellationToken)
        {
            PersonModel person = await _personRepository.GetPersonAsync(id, cancellationToken);
            if (person == null) return NotFound();
            return Ok(person);
        }

        [HttpPost]
        [Produces(typeof(PersonModel))]
        public async Task<IActionResult> PostAsync(PersonModel person, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            person = await _personRepository.CreatePersonAsync(person, cancellationToken);
            return Ok(person);
        }


        [HttpPost("{id}")]
        [Produces(typeof(PersonModel))]
        public async Task<IActionResult> PostAsync([FromRoute] long id, PersonModel person, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            person = await _personRepository.UpdatePersonAsync(id, person, cancellationToken);
            return Ok(person);
        }

        [HttpDelete("{id}")]
        [ProducesDefaultResponseType(typeof(IEnumerable<ErrorModel>))]
        public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var result = (await _personRepository.DeletePersonAsync(id, cancellationToken)).ToArray();
            if(result.Any())
            {
                return Ok(result);
            }

            return NoContent();
        }
    }
}
