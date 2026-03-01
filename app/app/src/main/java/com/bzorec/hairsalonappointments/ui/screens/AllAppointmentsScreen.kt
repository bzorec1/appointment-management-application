package com.bzorec.hairsalonappointments.ui.screens

import android.os.Build
import androidx.annotation.RequiresApi
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.compose.viewModel
import com.bzorec.hairsalonappointments.data.api.ApiClient
import com.bzorec.hairsalonappointments.data.model.AppointmentDto
import com.bzorec.hairsalonappointments.ui.util.isInPast
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import java.time.LocalDate
import java.time.ZoneId
import java.time.format.DateTimeFormatter

data class AllAppointmentsUiState(
    val appointments: List<AppointmentDto> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
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
class AllAppointmentsViewModel : ViewModel() {

    private val _uiState = MutableStateFlow(AllAppointmentsUiState())
    val uiState: StateFlow<AllAppointmentsUiState> = _uiState

    @RequiresApi(Build.VERSION_CODES.O)
    private val isoFormatter = DateTimeFormatter.ISO_OFFSET_DATE_TIME

    init {
        load()
    }

    @RequiresApi(Build.VERSION_CODES.O)
    fun load() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val zone = ZoneId.systemDefault()
                val today = LocalDate.now()
                val from = today.minusDays(7).atStartOfDay(zone).format(isoFormatter)
                val to = today.plusDays(30).atStartOfDay(zone).format(isoFormatter)

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

@RequiresApi(Build.VERSION_CODES.O)
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AllAppointmentsScreen(
    viewModel: AllAppointmentsViewModel = viewModel(),
    onNavigateToDetail: (Int) -> Unit = {}
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        contentWindowInsets = WindowInsets(0, 0, 0, 0),
        topBar = {
            TopAppBar(
                title = { Text("Vsi termini", fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { padding ->
        when {
            uiState.isLoading -> {
                Box(Modifier
                    .fillMaxSize()
                    .padding(padding)) {
                    CircularProgressIndicator(
                        modifier = Modifier.align(Alignment.Center),
                        color = MaterialTheme.colorScheme.primary
                    )
                }
            }

            uiState.error != null -> {
                Box(Modifier
                    .fillMaxSize()
                    .padding(padding)) {
                    Column(
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Text("\u26A0\uFE0F", fontSize = 36.sp)
                        Text(
                            uiState.error ?: "Neznana napaka",
                            color = MaterialTheme.colorScheme.error,
                            style = MaterialTheme.typography.bodyMedium
                        )
                        Button(onClick = { viewModel.load() }) {
                            Text("Poskusi znova")
                        }
                    }
                }
            }

            uiState.appointments.isEmpty() -> {
                Box(Modifier
                    .fillMaxSize()
                    .padding(padding)) {
                    Column(
                        modifier = Modifier
                            .align(Alignment.Center)
                            .padding(24.dp),
                        horizontalAlignment = Alignment.CenterHorizontally,
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        Text("\u2702\uFE0F", fontSize = 40.sp)
                        Text(
                            "Ni terminov",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.SemiBold
                        )
                    }
                }
            }

            else -> {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(padding),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(10.dp)
                ) {
                    if (uiState.upcoming.isNotEmpty()) {
                        item { SectionHeader(title = "Prihaja", count = uiState.upcoming.size) }
                        items(uiState.upcoming) { appointment ->
                            AppointmentCard(
                                appointment = appointment,
                                showDate = true,
                                onClick = { onNavigateToDetail(appointment.id) }
                            )
                        }
                    }

                    if (uiState.done.isNotEmpty()) {
                        item { SectionHeader(title = "Opravljeno", count = uiState.done.size) }
                        items(uiState.done) { appointment ->
                            AppointmentCard(
                                appointment = appointment,
                                showDate = true,
                                onClick = { onNavigateToDetail(appointment.id) }
                            )
                        }
                    }
                }
            }
        }
    }
}