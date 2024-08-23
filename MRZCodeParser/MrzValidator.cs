using System;
using System.Collections.Generic;
using System.Linq;

namespace MRZCodeParser
{
    public class MrzValidator
    {
        private readonly static string _charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly static int[] Weights = { 7, 3, 1 };
        private readonly static Dictionary<char, int> MappedSumValues = new Dictionary<char, int>();

        static MrzValidator()
        {
            for (int i = 0; i < _charset.Length; i++)
            {
                MappedSumValues[_charset[i]] = i;
            }
            MappedSumValues['<'] = 0;
            MappedSumValues[' '] = 0;
            MappedSumValues[','] = 0;
        }

        public static bool Validate(MrzCode mrz)
        {
            try
            {
                return mrz.Type switch
                {
                    CodeType.TD1 => ValidateTD1(mrz),
                    CodeType.TD2 => ValidateTD2(mrz),
                    CodeType.TD3 => ValidateTD3(mrz),
                    CodeType.MRVA => ValidateMRV(mrz),
                    CodeType.MRVB => ValidateMRV(mrz),
                    _ => ValidateUnknown()
                };
            }
            catch
            {
                return false;
            }
        }

        private static bool ValidateTD1(MrzCode mrz)
        {
            var isDocumentNumberValid = IsFieldValid(mrz[FieldType.DocumentNumber], mrz[FieldType.DocumentNumberCheckDigit]);
            var isBirthDateValid = IsFieldValid(mrz[FieldType.BirthDate], mrz[FieldType.BirthDateCheckDigit]);
            var isExpiryDateValid = IsFieldValid(mrz[FieldType.ExpiryDate], mrz[FieldType.ExpiryDateCheckDigit]);

            var firstMrzLine = mrz.Lines.First();
            var secondMrzLine = mrz.Lines.Skip(1).First();
            var mrzCheckDigit = (int)char.GetNumericValue(mrz[FieldType.OverallCheckDigit][0]);
            var calculatedCheckSum = 0;
            calculatedCheckSum += CalculateSum(firstMrzLine.Value[5..30]);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[..7], 25);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[8..15], 32);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[18..29], 43);
            var mrzCheckSumIsValid = calculatedCheckSum % 10 == mrzCheckDigit;

            return isDocumentNumberValid && isBirthDateValid && isExpiryDateValid && mrzCheckSumIsValid;
        }

        private static bool ValidateTD2(MrzCode mrz)
        {
            var isDocumentNumberValid = IsFieldValid(mrz[FieldType.DocumentNumber], mrz[FieldType.DocumentNumberCheckDigit]);
            var isBirthDateValid = IsFieldValid(mrz[FieldType.BirthDate], mrz[FieldType.BirthDateCheckDigit]);
            var isExpiryDateValid = IsFieldValid(mrz[FieldType.ExpiryDate], mrz[FieldType.ExpiryDateCheckDigit]);

            var secondMrzLine = mrz.Lines.Last();
            var mrzCheckDigit = (int)char.GetNumericValue(mrz[FieldType.OverallCheckDigit][0]);
            var calculatedCheckSum = 0;
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[..10]);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[13..20], 10);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[21..35], 17);
            var mrzCheckSumIsValid = calculatedCheckSum % 10 == mrzCheckDigit;


            // temp fix for MDA BO 01001 ids with no expiry date
            if (mrz[FieldType.CountryCode] == "MDA" &&  string.IsNullOrEmpty(mrz[FieldType.ExpiryDate]))
            {
                return isDocumentNumberValid && isBirthDateValid && isExpiryDateValid;
            }

            return isDocumentNumberValid && isBirthDateValid && isExpiryDateValid && mrzCheckSumIsValid;
        }

        private static bool ValidateTD3(MrzCode mrz)
        {
            var isDocumentNumberValid = IsFieldValid(mrz[FieldType.DocumentNumber], mrz[FieldType.DocumentNumberCheckDigit]);
            var isBirthDateValid = IsFieldValid(mrz[FieldType.BirthDate], mrz[FieldType.BirthDateCheckDigit]);
            var isExpiryDateValid = IsFieldValid(mrz[FieldType.ExpiryDate], mrz[FieldType.ExpiryDateCheckDigit]);
            var isOptionalDataValid = IsFieldValid(mrz[FieldType.OptionalData], mrz[FieldType.OptionalDataCheckDigit]);

            var secondMrzLine = mrz.Lines.Last();
            var mrzCheckDigit = (int)char.GetNumericValue(mrz[FieldType.OverallCheckDigit][0]);
            var calculatedCheckSum = 0;
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[..10]);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[13..20], 10);
            calculatedCheckSum += CalculateSum(secondMrzLine.Value[21..43], 17);
            var mrzCheckSumIsValid = calculatedCheckSum % 10 == mrzCheckDigit;

            return isDocumentNumberValid && isBirthDateValid && isExpiryDateValid && isOptionalDataValid && mrzCheckSumIsValid;
        }

        private static bool ValidateMRV(MrzCode mrz)
        {
            var isDocumentNumberValid = IsFieldValid(mrz[FieldType.DocumentNumber], mrz[FieldType.DocumentNumberCheckDigit]);
            var isBirthDateValid = IsFieldValid(mrz[FieldType.BirthDate], mrz[FieldType.BirthDateCheckDigit]);
            var isExpiryDateValid = IsFieldValid(mrz[FieldType.ExpiryDate], mrz[FieldType.ExpiryDateCheckDigit]);

            return isDocumentNumberValid && isBirthDateValid && isExpiryDateValid;
        }

        private static bool ValidateUnknown()
        {
            return false;
        }

        private static int CalculateSum(string field, int startWeight = 0)
        {
            int sum = 0;
            for (int i = 0, j = startWeight; i < field.Length; ++i, ++j)
            {
                sum += MappedSumValues[field[i]] * Weights[j % 3];
            }
            return sum;
        }

        private static bool IsFieldValid(string fieldValue, string checkSum)
        {
            if (checkSum.Length != 1)
                return false;

            var checkSumNumber = (int)char.GetNumericValue(checkSum[0]);

            var calculatedCheckSum = CalculateSum(fieldValue);
            return calculatedCheckSum % 10 == checkSumNumber;
        }
    }
}