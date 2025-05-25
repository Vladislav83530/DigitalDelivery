import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'phoneFormat',
    standalone: true
})
export class PhoneFormatPipe implements PipeTransform {
    transform(value: string): string {
        if (!value) return '';
        
        // Remove all non-digit characters
        const digits = value.replace(/\D/g, '');
        
        // Format: +38 (XXX) XXX-XX-XX
        if (digits.length === 12) { // Ukrainian number with country code
            return `+${digits.slice(0, 2)} (${digits.slice(2, 5)}) ${digits.slice(5, 8)}-${digits.slice(8, 10)}-${digits.slice(10)}`;
        } else if (digits.length === 10) { // Ukrainian number without country code
            return `+38 (${digits.slice(0, 3)}) ${digits.slice(3, 6)}-${digits.slice(6, 8)}-${digits.slice(8)}`;
        }
        
        // Return original value if format doesn't match
        return value;
    }
} 