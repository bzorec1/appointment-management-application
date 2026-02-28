package com.bzorec.hairsalonappointments.ui.util

import java.time.OffsetDateTime
import java.time.format.DateTimeFormatter
import java.util.Locale

private val timeFmt      = DateTimeFormatter.ofPattern("HH:mm")
private val dateFmt      = DateTimeFormatter.ofPattern("EEEE, d. MMMM yyyy", Locale("sl"))
private val shortDateFmt = DateTimeFormatter.ofPattern("d. MMM", Locale("sl"))

fun parseTime(iso: String): String = try {
    OffsetDateTime.parse(iso).format(timeFmt)
} catch (_: Exception) {
    iso.take(16).takeLast(5)
}

fun parseDate(iso: String): String = try {
    OffsetDateTime.parse(iso).format(dateFmt)
} catch (_: Exception) {
    iso.take(10)
}

fun parseShortDate(iso: String): String = try {
    OffsetDateTime.parse(iso).format(shortDateFmt)
} catch (_: Exception) {
    iso.take(10)
}

fun isInPast(iso: String): Boolean = try {
    OffsetDateTime.parse(iso).isBefore(OffsetDateTime.now())
} catch (_: Exception) {
    false
}
