/**
 * Formats a Date object or ISO string into 'YYYY-MM-DDThh:mm' in the user's local timezone.
 * Suitable for <input type="datetime-local"> binding.
 */
function toLocalDatetimeString(dateInput) {
    if (!dateInput) return '';
    const d = new Date(dateInput);
    if (isNaN(d.getTime())) return ''; // Return empty string if date is invalid

    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
}