export function formatUAPhoneNumber(raw: string): string {
    let value = raw.replace(/\D/g, '');

    if (value.startsWith('380')) {
        value = '+' + value;
    } else if (value.startsWith('0')) {
        value = '+380' + value.slice(1);
    } else {
        value = '+380';
    }

    if (value.length > 4) value = value.slice(0, 4) + ' (' + value.slice(4);
    if (value.length > 8) value = value.slice(0, 8) + ') ' + value.slice(8);
    if (value.length > 13) value = value.slice(0, 13) + '-' + value.slice(13);
    if (value.length > 16) value = value.slice(0, 16) + '-' + value.slice(16);

    return value.slice(0, 19);
}