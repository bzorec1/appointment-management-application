package com.bzorec.hairsalonappointments.ui.screens

import android.content.Intent
import android.os.Build
import android.provider.CalendarContract
import androidx.annotation.RequiresApi
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.bzorec.hairsalonappointments.data.api.ApiClient
import com.bzorec.hairsalonappointments.data.model.AppointmentDto
import com.bzorec.hairsalonappointments.ui.util.parseDate
import com.bzorec.hairsalonappointments.ui.util.parseTime
import java.time.OffsetDateTime

@RequiresApi(Build.VERSION_CODES.O)
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AppointmentDetailScreen(
    appointmentId: Int,
    onNavigateBack: () -> Unit
) {
    var appointment by remember { mutableStateOf<AppointmentDto?>(null) }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(appointmentId) {
        try {
            val response = ApiClient.apiService.getAppointmentById(appointmentId)
            if (response.isSuccessful) {
                appointment = response.body()
            } else {
                error = "Napaka: ${response.code()}"
            }
        } catch (e: Exception) {
            error = "Napaka pri povezavi: ${e.message}"
        } finally {
            isLoading = false
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(
                        "Podrobnosti termina",
                        fontWeight = FontWeight.Bold
                    )
                },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Nazaj")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer,
                    navigationIconContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            when {
                isLoading -> CircularProgressIndicator(
                    modifier = Modifier.align(Alignment.Center),
                    color = MaterialTheme.colorScheme.primary
                )

                error != null -> Column(
                    modifier = Modifier
                        .align(Alignment.Center)
                        .padding(24.dp),
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    Text("⚠\uFE0F", fontSize = 36.sp)
                    Text(error ?: "Napaka", color = MaterialTheme.colorScheme.error)
                    OutlinedButton(onClick = onNavigateBack) { Text("Nazaj") }
                }

                appointment != null -> AppointmentDetailContent(
                    appointment = appointment!!,
                    modifier = Modifier.fillMaxSize()
                )
            }
        }
    }
}

@RequiresApi(Build.VERSION_CODES.O)
@Composable
private fun AppointmentDetailContent(
    appointment: AppointmentDto,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current

    val startDt = try {
        OffsetDateTime.parse(appointment.start)
    } catch (_: Exception) {
        null
    }
    val endDt = try {
        OffsetDateTime.parse(appointment.end)
    } catch (_: Exception) {
        null
    }

    val dateStr = parseDate(appointment.start)
    val startStr = parseTime(appointment.start)
    val endStr = parseTime(appointment.end)

    Column(
        modifier = modifier
            .verticalScroll(rememberScrollState())
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(14.dp)
    ) {
        ElevatedCard(
            modifier = Modifier.fillMaxWidth(),
            colors = CardDefaults.elevatedCardColors(
                containerColor = MaterialTheme.colorScheme.primaryContainer
            )
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(20.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.spacedBy(4.dp)
            ) {
                Text(
                    "$startStr – $endStr",
                    style = MaterialTheme.typography.headlineMedium,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.onPrimaryContainer
                )
                Text(
                    dateStr,
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.75f)
                )
            }
        }

        ElevatedCard(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.padding(4.dp)) {
                DetailRow(label = "Storitev", value = appointment.service)
                HorizontalDivider(
                    modifier = Modifier.padding(horizontal = 16.dp),
                    thickness = 0.5.dp
                )
                if (appointment.resourceName != null) {
                    DetailRow(label = "Frizer", value = appointment.resourceName)
                    HorizontalDivider(
                        modifier = Modifier.padding(horizontal = 16.dp),
                        thickness = 0.5.dp
                    )
                }
                DetailRow(label = "Stranka", value = appointment.customerName ?: "–")
                if (appointment.phone.isNotBlank()) {
                    HorizontalDivider(
                        modifier = Modifier.padding(horizontal = 16.dp),
                        thickness = 0.5.dp
                    )
                    DetailRow(label = "Telefon", value = appointment.phone)
                }
            }
        }

        Button(
            onClick = {
                val intent =
                    Intent(Intent.ACTION_INSERT, CalendarContract.Events.CONTENT_URI).apply {
                        putExtra(CalendarContract.Events.TITLE, appointment.title)
                        putExtra(
                            CalendarContract.Events.DESCRIPTION,
                            "Storitev: ${appointment.service}\nStranka: ${appointment.customerName}\nTel: ${appointment.phone}"
                        )
                        startDt?.let {
                            putExtra(
                                CalendarContract.EXTRA_EVENT_BEGIN_TIME,
                                it.toInstant().toEpochMilli()
                            )
                        }
                        endDt?.let {
                            putExtra(
                                CalendarContract.EXTRA_EVENT_END_TIME,
                                it.toInstant().toEpochMilli()
                            )
                        }
                    }
                context.startActivity(intent)
            },
            modifier = Modifier
                .fillMaxWidth()
                .height(52.dp),
            shape = RoundedCornerShape(14.dp)
        ) {
            Icon(
                Icons.Default.DateRange,
                contentDescription = null,
                modifier = Modifier.size(20.dp)
            )
            Spacer(Modifier.width(8.dp))
            Text("Dodaj v koledar", fontWeight = FontWeight.SemiBold)
        }

        if (!appointment.smsPreview.isNullOrBlank()) {
            SmsBubble(text = appointment.smsPreview)
        }
    }
}

@Composable
private fun DetailRow(label: String, value: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 13.dp),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            label,
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            fontWeight = FontWeight.Medium
        )
        Text(
            value,
            style = MaterialTheme.typography.bodyMedium,
            fontWeight = FontWeight.SemiBold
        )
    }
}

@Composable
private fun SmsBubble(text: String) {
    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        Row(
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(6.dp)
        ) {
            Box(
                modifier = Modifier
                    .size(6.dp)
                    .clip(RoundedCornerShape(50))
                    .background(MaterialTheme.colorScheme.secondary)
            )
            Text(
                "Predogled SMS sporočila",
                style = MaterialTheme.typography.labelMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                fontWeight = FontWeight.Medium
            )
        }

        Surface(
            shape = RoundedCornerShape(
                topStart = 18.dp,
                topEnd = 18.dp,
                bottomEnd = 4.dp,
                bottomStart = 18.dp
            ),
            color = MaterialTheme.colorScheme.surfaceVariant,
            modifier = Modifier.widthIn(max = 340.dp)
        ) {
            Text(
                text,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 12.dp),
                style = MaterialTheme.typography.bodySmall,
                lineHeight = 20.sp,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}