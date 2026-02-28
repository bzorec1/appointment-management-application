package com.bzorec.hairsalonappointments.data.api

import com.bzorec.hairsalonappointments.data.model.*
import retrofit2.Response
import retrofit2.http.*

interface ApiService {

    @GET("appointments")
    suspend fun getAppointments(
        @Query("from") from: String,
        @Query("to") to: String,
        @Query("api-version") apiVersion: String = "1.0"
    ): Response<List<AppointmentDto>>

    @POST("appointments")
    suspend fun createAppointment(
        @Body appointment: NewAppointment,
        @Query("api-version") apiVersion: String = "1.0"
    ): Response<AppointmentDto>

    @GET("appointments/{id}")
    suspend fun getAppointmentById(
        @Path("id") id: Int,
        @Query("api-version") apiVersion: String = "1.0"
    ): Response<AppointmentDto>

    @POST("suggestions")
    suspend fun getSuggestions(
        @Body request: SuggestionRequest,
        @Query("api-version") apiVersion: String = "1.0"
    ): Response<List<SuggestionSlot>>
}
