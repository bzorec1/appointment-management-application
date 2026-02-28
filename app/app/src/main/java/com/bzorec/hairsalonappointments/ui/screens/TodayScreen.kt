package com.bzorec.hairsalonappointments.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.KeyboardArrowLeft
import androidx.compose.material.icons.filled.KeyboardArrowRight
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.bzorec.hairsalonappointments.data.model.AppointmentDto
import com.bzorec.hairsalonappointments.ui.theme.StylistAnaTeal
import com.bzorec.hairsalonappointments.ui.theme.StylistAnaTealContainer
import com.bzorec.hairsalonappointments.ui.theme.StylistDefaultGrey
import com.bzorec.hairsalonappointments.ui.theme.StylistDefaultGreyContainer
import com.bzorec.hairsalonappointments.ui.theme.StylistTinaAmber
import com.bzorec.hairsalonappointments.ui.theme.StylistTinaAmberContainer
import com.bzorec.hairsalonappointments.ui.util.isInPast
import com.bzorec.hairsalonappointments.ui.util.parseShortDate
import com.bzorec.hairsalonappointments.ui.util.parseTime
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TodayScreen(
    viewModel: AppointmentsViewModel = viewModel(),
    onNavigateToNew: () -> Unit = {},
    onNavigateToDetail: (Int) -> Unit = {}
) {
    val uiState by viewModel.uiState.collectAsState()
    val isToday = uiState.selectedDate == LocalDate.now()

    Scaffold(
        contentWindowInsets = WindowInsets(0, 0, 0, 0),
        topBar = {
            TopAppBar(
                title = {
                    Column {
                        Text(
                            if (isToday) "Današnji termini" else "Termini",
                            fontWeight = FontWeight.Bold
                        )
                        Text(
                            uiState.selectedDate
                                .format(DateTimeFormatter.ofPattern("EEEE, d. MMMM", Locale("sl"))),
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.loadAppointmentsForDate(uiState.selectedDate.minusDays(1)) }) {
                        Icon(Icons.Default.KeyboardArrowLeft, contentDescription = "Prejšnji dan")
                    }
                    IconButton(onClick = { viewModel.loadTodayAppointments() }) {
                        Icon(Icons.Default.Refresh, contentDescription = "Danes")
                    }
                    IconButton(onClick = { viewModel.loadAppointmentsForDate(uiState.selectedDate.plusDays(1)) }) {
                        Icon(Icons.Default.KeyboardArrowRight, contentDescription = "Naslednji dan")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        },
        floatingActionButton = {
            FloatingActionButton(
                onClick = onNavigateToNew,
                containerColor = MaterialTheme.colorScheme.primary
            ) {
                Icon(
                    Icons.Default.Add,
                    contentDescription = "Dodaj termin",
                    tint = MaterialTheme.colorScheme.onPrimary
                )
            }
        }
    ) { padding ->
        when {
            uiState.isLoading && uiState.appointments.isEmpty() -> {
                Box(Modifier.fillMaxSize().padding(padding)) {
                    CircularProgressIndicator(
                        modifier = Modifier.align(Alignment.Center),
                        color = MaterialTheme.colorScheme.primary
                    )
                }
            }

            uiState.error != null -> {
                Box(Modifier.fillMaxSize().padding(padding)) {
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
                        Button(onClick = { viewModel.loadTodayAppointments() }) {
                            Text("Poskusi znova")
                        }
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
                    item {
                        StatsRow(
                            total = uiState.appointments.size,
                            upcoming = uiState.upcoming.size,
                            done = uiState.done.size
                        )
                        Spacer(Modifier.height(4.dp))
                    }

                    if (uiState.appointments.isEmpty()) {
                        item {
                            Column(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 40.dp),
                                horizontalAlignment = Alignment.CenterHorizontally,
                                verticalArrangement = Arrangement.spacedBy(10.dp)
                            ) {
                                Text("\u2702\uFE0F", fontSize = 40.sp)
                                Text(
                                    if (isToday) "Danes ni terminov" else "Ni terminov za ta dan",
                                    style = MaterialTheme.typography.titleMedium,
                                    fontWeight = FontWeight.SemiBold
                                )
                                Text(
                                    "Pritisni + za dodajanje novega termina",
                                    style = MaterialTheme.typography.bodySmall,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant
                                )
                            }
                        }
                    } else {
                        if (uiState.upcoming.isNotEmpty()) {
                            item { SectionHeader(title = "Prihaja", count = uiState.upcoming.size) }
                            items(uiState.upcoming) { appointment ->
                                AppointmentCard(
                                    appointment = appointment,
                                    onClick = { onNavigateToDetail(appointment.id) }
                                )
                            }
                        }

                        if (uiState.done.isNotEmpty()) {
                            item { SectionHeader(title = "Opravljeno", count = uiState.done.size) }
                            items(uiState.done) { appointment ->
                                AppointmentCard(
                                    appointment = appointment,
                                    onClick = { onNavigateToDetail(appointment.id) }
                                )
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun StatsRow(total: Int, upcoming: Int, done: Int) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(10.dp)
    ) {
        StatCard(
            label = "Skupaj",
            value = total,
            modifier = Modifier.weight(1f)
        )
        StatCard(
            label = "Prihaja",
            value = upcoming,
            modifier = Modifier.weight(1f),
            valueColor = MaterialTheme.colorScheme.primary
        )
        StatCard(
            label = "Opravljeno",
            value = done,
            modifier = Modifier.weight(1f),
            valueColor = StylistAnaTeal
        )
    }
}

@Composable
private fun StatCard(
    label: String,
    value: Int,
    modifier: Modifier = Modifier,
    valueColor: Color = MaterialTheme.colorScheme.onSurface
) {
    ElevatedCard(modifier = modifier) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(2.dp)
        ) {
            Text(
                value.toString(),
                style = MaterialTheme.typography.headlineMedium,
                fontWeight = FontWeight.ExtraBold,
                color = valueColor
            )
            Text(
                label,
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

@Composable
fun SectionHeader(title: String, count: Int) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 4.dp, bottom = 2.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        Text(
            title.uppercase(),
            style = MaterialTheme.typography.labelMedium,
            fontWeight = FontWeight.Bold,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            letterSpacing = 0.8.sp
        )
        Surface(
            shape = RoundedCornerShape(50),
            color = MaterialTheme.colorScheme.primaryContainer
        ) {
            Text(
                count.toString(),
                modifier = Modifier.padding(horizontal = 8.dp, vertical = 2.dp),
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onPrimaryContainer,
                fontWeight = FontWeight.Bold
            )
        }
        HorizontalDivider(modifier = Modifier.weight(1f), thickness = 0.5.dp)
    }
}

private fun stylistColor(resourceId: Int): Color = when (resourceId) {
    1    -> StylistAnaTeal
    2    -> StylistTinaAmber
    else -> StylistDefaultGrey
}

private fun stylistColorContainer(resourceId: Int): Color = when (resourceId) {
    1    -> StylistAnaTealContainer
    2    -> StylistTinaAmberContainer
    else -> StylistDefaultGreyContainer
}

@Composable
fun AppointmentCard(
    appointment: AppointmentDto,
    showDate: Boolean = false,
    onClick: () -> Unit
) {
    val isFulfilled = appointment.status == "Fulfilled" || isInPast(appointment.end)
    val startTime = parseTime(appointment.start)
    val endTime   = parseTime(appointment.end)

    val accentColor = stylistColor(appointment.resourceId)
        .let { if (isFulfilled) it.copy(alpha = 0.4f) else it }
    val containerColor = stylistColorContainer(appointment.resourceId)

    ElevatedCard(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth(),
        elevation = CardDefaults.elevatedCardElevation(defaultElevation = if (isFulfilled) 0.dp else 1.dp)
    ) {
        Row(modifier = Modifier.fillMaxWidth()) {
            Box(
                modifier = Modifier
                    .width(5.dp)
                    .fillMaxHeight()
                    .background(accentColor)
            )

            Box(
                modifier = Modifier
                    .background(containerColor)
                    .padding(horizontal = 14.dp, vertical = 16.dp),
                contentAlignment = Alignment.Center
            ) {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    if (showDate) {
                        Text(
                            parseShortDate(appointment.start),
                            style = MaterialTheme.typography.labelSmall,
                            color = accentColor.copy(alpha = 0.7f)
                        )
                    }
                    Text(
                        startTime,
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = accentColor
                    )
                    Text(
                        endTime,
                        style = MaterialTheme.typography.bodySmall,
                        color = accentColor.copy(alpha = 0.7f)
                    )
                }
            }

            Column(
                modifier = Modifier
                    .weight(1f)
                    .padding(horizontal = 14.dp, vertical = 12.dp),
                verticalArrangement = Arrangement.spacedBy(4.dp)
            ) {
                Text(
                    appointment.customerName ?: "Neznana stranka",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.SemiBold,
                    color = if (isFulfilled) MaterialTheme.colorScheme.onSurface.copy(alpha = 0.5f)
                            else MaterialTheme.colorScheme.onSurface
                )
                Text(
                    appointment.service,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                if (appointment.resourceName != null) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        Box(
                            modifier = Modifier
                                .size(7.dp)
                                .clip(RoundedCornerShape(50))
                                .background(accentColor)
                        )
                        Text(
                            appointment.resourceName,
                            style = MaterialTheme.typography.labelSmall,
                            color = accentColor,
                            fontWeight = FontWeight.Medium
                        )
                    }
                }
                if (isFulfilled) {
                    Text(
                        "\u2713 Opravljeno",
                        style = MaterialTheme.typography.labelSmall,
                        color = MaterialTheme.colorScheme.primary.copy(alpha = 0.6f),
                        fontWeight = FontWeight.Medium
                    )
                }
            }
        }
    }
}
