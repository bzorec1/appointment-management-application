package com.bzorec.hairsalonappointments.ui.util

import android.os.Build
import androidx.annotation.RequiresApi
import java.time.OffsetDateTime
import java.time.format.DateTimeFormatter
import java.util.Locale

@RequiresApi(Build.VERSION_CODES.O)
private val timeFmt = DateTimeFormatter.ofPattern("HH:mm")

@RequiresApi(Build.VERSION_CODES.O)
private val dateFmt = DateTimeFormatter.ofPattern("EEEE, d. MMMM yyyy", Locale("sl"))

@RequiresApi(Build.VERSION_CODES.O)
private val shortDateFmt = DateTimeFormatter.ofPattern("d. MMM", Locale("sl"))

@RequiresApi(Build.VERSION_CODES.O)
fun parseTime(iso: String): String = try {
    OffsetDateTime.parse(iso).format(timeFmt)
} catch (_: Exception) {
    iso.take(16).takeLast(5)
}

@RequiresApi(Build.VERSION_CODES.O)
fun parseDate(iso: String): String = try {
    OffsetDateTime.parse(iso).format(dateFmt)
} catch (_: Exception) {
    iso.take(10)
}

@RequiresApi(Build.VERSION_CODES.O)
fun parseShortDate(iso: String): String = try {
    OffsetDateTime.parse(iso).format(shortDateFmt)
} catch (_: Exception) {
    iso.take(10)
}

@RequiresApi(Build.VERSION_CODES.O)
fun isInPast(iso: String): Boolean = try {
    OffsetDateTime.parse(iso).isBefore(OffsetDateTime.now())
} catch (_: Exception) {
    false
}
