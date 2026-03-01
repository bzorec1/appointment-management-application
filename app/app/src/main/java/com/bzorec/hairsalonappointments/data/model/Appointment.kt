package com.bzorec.hairsalonappointments.data.model

data class AppointmentDto(
    val id: Int,
    val title: String,
    val start: String,
    val end: String,
    val resourceId: Int,
    val resourceName: String?,
    val phone: String,
    val service: String,
    val customerName: String?,
    val createdAt: String,
    val status: String? = null,
    val customerGoogleCalendarUrl: String? = null,
    val customerIcsUrl: String? = null,
    val smsPreview: String? = null
)

data class NewAppointment(
    val title: String,
    val start: String,
    val end: String,
    val resourceId: Int,
    val phone: String,
    val service: String,
    val customerName: String
)

data class TimeWindow(
    val from: String,
    val to: String
)