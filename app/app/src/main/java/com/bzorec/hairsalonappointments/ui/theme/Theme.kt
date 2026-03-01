package com.bzorec.hairsalonappointments.ui.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val LightColorScheme = lightColorScheme(
    primary = Rose40,
    onPrimary = Color.White,
    primaryContainer = Rose90,
    onPrimaryContainer = Rose10,
    secondary = Mauve40,
    onSecondary = Color.White,
    secondaryContainer = Mauve90,
    onSecondaryContainer = Mauve10,
    tertiary = Caramel40,
    onTertiary = Color.White,
    tertiaryContainer = Caramel90,
    onTertiaryContainer = Caramel10,
    background = Neutral99,
    onBackground = Neutral10,
    surface = Neutral99,
    onSurface = Neutral10,
    surfaceVariant = NeutralVar90,
    onSurfaceVariant = NeutralVar40,
    outline = NeutralVar40,
    outlineVariant = NeutralVar80,
    error = Error40,
    onError = Color.White,
    errorContainer = Error90,
    onErrorContainer = Error10,
    inverseSurface = Neutral20,
    inverseOnSurface = Neutral90,
    inversePrimary = Rose80,
)

private val DarkColorScheme = darkColorScheme(
    primary = Rose80,
    onPrimary = Rose20,
    primaryContainer = Rose30,
    onPrimaryContainer = Rose90,
    secondary = Mauve80,
    onSecondary = Mauve20,
    secondaryContainer = Mauve30,
    onSecondaryContainer = Mauve90,
    tertiary = Caramel80,
    onTertiary = Caramel20,
    tertiaryContainer = Caramel40,
    onTertiaryContainer = Caramel90,
    background = Neutral10,
    onBackground = Neutral90,
    surface = Neutral10,
    onSurface = Neutral90,
    surfaceVariant = NeutralVar30,
    onSurfaceVariant = NeutralVar80,
    outline = NeutralVar80,
    outlineVariant = NeutralVar30,
    error = Error80,
    onError = Error20,
    errorContainer = Error30,
    onErrorContainer = Error90,
    inverseSurface = Neutral90,
    inverseOnSurface = Neutral20,
    inversePrimary = Rose40,
)

@Composable
fun HairSalonAppointmentsTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}
