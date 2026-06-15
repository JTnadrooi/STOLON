using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public sealed class EntityIdAttribute : ValidationAttribute
    {
        private static readonly EntityDefinition[]? s_entities;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (s_entities is null) return ValidationResult.Success;

            string input = value.ToString()!;
            bool exists = s_entities.Any(e => e.Id == input);
            return exists ? ValidationResult.Success : new ValidationResult($"Entity '{input}' does not exist.");
        }

        static EntityIdAttribute()
        {
            if (STOLON.IsInitiated)
            {
                s_entities = STOLON.Services.Resolve<EntityDefinition[]>();
            }
        }
    }
}
