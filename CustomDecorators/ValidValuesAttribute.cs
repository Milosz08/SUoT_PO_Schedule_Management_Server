﻿using System.Linq;
using System.ComponentModel.DataAnnotations;


namespace asp_net_po_schedule_management_server.CustomDecorators
{
    // klasa nadpisująca domyślny dekorator do walidacji na dekorator przyjmujący wiele parametrów
    // w postaci tablicy stringów (tablica musi być pre-kompilowana, czyli albo const albo na stałe zapisana)
    public class ValidValuesAttribute : ValidationAttribute
    {
        private string[] _args;

        public ValidValuesAttribute(params string[] args)
        {
            _args = args;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (_args.Contains(value as string)) {
                return ValidationResult.Success;
            }
            return new ValidationResult("Podana wartość nie jest zadeklarowana jako wartość akceptowalna");
        }
    }
}