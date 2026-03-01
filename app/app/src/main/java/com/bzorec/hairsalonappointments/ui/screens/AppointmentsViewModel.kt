package com.bzorec.hairsalonappointments.ui.screens

import android.os.Build
import androidx.annotation.RequiresApi
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.bzorec.hairsalonappointments.data.api.ApiClient
import com.bzorec.hairsalonappointments.data.model.*
import com.bzorec.hairsalonappointments.ui.util.isInPast
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import java.time.LocalDate
import java.time.ZoneId
import java.time.format.DateTimeFormatter

@RequiresApi(Build.VERSION_CODES.O)
data class AppointmentsUiState @RequiresApi(Build.VERSION_CODES.O) constructor(
    val appointments: List<AppointmentDto> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null,
    val selectedDate: LocalDate = LocalDate.now()
) {
    val upcoming: List<AppointmentDto>
        @RequiresApi(Build.VERSION_CODES.O)
        get() = appointments.filter {
            it.status != "Fulfilled" && !isInPast(
                it.end
            )
        }
    val done: List<AppointmentDto>
        @RequiresApi(Build.VERSION_CODES.O)
        get() = appointments.filter {
            it.status == "Fulfilled" || isInPast(
                it.end
            )
        }
}

@RequiresApi(Build.VERSION_CODES.O)
class AppointmentsViewModel : ViewModel() {

    @RequiresApi(Build.VERSION_CODES.O)
    private val _uiState = MutableStateFlow(AppointmentsUiState())
    val uiState: StateFlow<AppointmentsUiState> = _uiState

    @RequiresApi(Build.VERSION_CODES.O)
    private val isoFormatter = DateTimeFormatter.ISO_OFFSET_DATE_TIME

    init {
        loadTodayAppointments()
        viewModelScope.launch {
            while (true) {
                delay(10_000L)
                loadAppointmentsForDate(_uiState.value.selectedDate)
            }
        }
    }

    fun loadTodayAppointments() = loadAppointmentsForDate(LocalDate.now())

    fun loadAppointmentsForDate(date: LocalDate) {
        viewModelScope.launch {
            _uiState.value =
                _uiState.value.copy(isLoading = true, error = null, selectedDate = date)

            try {
                val zone = ZoneId.systemDefault()
                val from = date.atStartOfDay(zone).format(isoFormatter)
                val to = date.plusDays(1).atStartOfDay(zone).format(isoFormatter)

                val response = ApiClient.apiService.getAppointments(from, to)

                if (response.isSuccessful) {
                    _uiState.value = _uiState.value.copy(
                        appointments = (response.body() ?: emptyList()).sortedBy { it.start },
                        isLoading = false
                    )
                } else {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = "Napaka: ${response.code()}"
                    )
                }
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = "Napaka pri povezavi: ${e.message}"
                )
            }
        }
    }
}