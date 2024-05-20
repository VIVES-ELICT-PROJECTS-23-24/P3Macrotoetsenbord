#include "stm32f091xc.h"
#include "stdio.h"
#include "stdbool.h"
#include "usart2.h"
#include "ad.h"
#include "buttons.h"

#define ROWS 3           
#define COLS 3

bool druk = 0;
int yur = 0;
int outputValue = 0;
int outputValueVorige = 11;
int FirstRun = 0;

// Functie prototypes.
void SystemClock_Config(void);
void InitIo(void);
void WaitForMs(uint32_t timespan);
static uint16_t adValue = 0;
static char text[101];

static uint8_t count = 0;
static volatile uint32_t ticks = 0;

char* itoa(int value, char* result, int base) {
    // Check for valid base
    if (base < 2 || base > 36) {
        *result = '\0';
        return result;
    }

    char* ptr = result, *ptr1 = result, tmp_char;
    int tmp_value;

    // Convert integer to string
    do {
        tmp_value = value;
        value /= base;
        *ptr++ = "ZYXWVUTSRQPONMLKJIHGFEDCBA9876543210123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[35 + (tmp_value - value * base)];
    } while (value);

    // Add negative sign if necessary
    if (tmp_value < 0)
        *ptr++ = '-';
    *ptr-- = '\0';

    // Reverse the string
    while (ptr1 < ptr) {
        tmp_char = *ptr;
        *ptr-- = *ptr1;
        *ptr1++ = tmp_char;
    }

    return result;
}

// Initialize GPIO pins for rows (output)
void initRows() 
{
    RCC->AHBENR |= RCC_AHBENR_GPIOAEN;  // Enable GPIOA clock

    // Configure pins A7, A6, and A8 as output with pull-up resistors
    GPIOA->MODER &= ~(GPIO_MODER_MODER7 | GPIO_MODER_MODER6 | GPIO_MODER_MODER8);
	  GPIOA->MODER |= GPIO_MODER_MODER7_0 | GPIO_MODER_MODER6_0 | GPIO_MODER_MODER8_0;
//   GPIOA->PUPDR |= GPIO_PUPDR_PUPDR7_0 | GPIO_PUPDR_PUPDR6_0 | GPIO_PUPDR_PUPDR8_0;
}

// Initialize GPIO pins for columns (inputs)
void initCols() 
{
    // Configure pins A5, A1, and A4 as inputs
    GPIOA->MODER &= ~(GPIO_MODER_MODER5 | GPIO_MODER_MODER1 | GPIO_MODER_MODER4);
    GPIOA->MODER |= GPIO_MODER_MODER5_0 | GPIO_MODER_MODER1_0 | GPIO_MODER_MODER4_0;
}

// Function to scan the matrix
void scanMatrix(int matrix[ROWS][COLS]) 
{
	  int inputs[3] = {7,6,8};
		int outputs[3] = {5,1,4};
		char buffer[40]; // Buffer to hold the converted row and column
    //loop de rijen
		for (int i = 0; i < ROWS; i++) 
		{
        //maak ieder rij hoog om de beurt
        GPIOA->ODR = (1 << inputs[i]); // Set the ith row pin high
        
        // Check kolommen voor ingedrukte knop
        for (int j = 0; j < COLS; j++)
				{
            // If button pressed, matrix[i][j] will be 1
            matrix[i][j] = (GPIOA->IDR >> outputs[j]) & 0x00000001;
					if (druk == 0)
					{
						if (matrix[i][j] == 1) 
						{
								itoa(GPIOA->IDR,buffer,16);
								if(matrix[0][0]==1)
								{
									if (FirstRun == 1)
									{
									StringToUsart2("1");
									}
								}
								else if (matrix[0][1]==1)
								{
									StringToUsart2("2");
								}
								else if (matrix[0][2]==1)
								{
									StringToUsart2("3");
								}
								else if (matrix[1][0]==1)
								{
									StringToUsart2("4");
								}
								else if (matrix[1][1]==1)
								{
									StringToUsart2("5");
								}
								else if (matrix[1][2]==1)
								{
									StringToUsart2("6");
								}
								else if (matrix[2][0]==1)
								{
									StringToUsart2("7");
								}
								else if (matrix[2][1]==1)
								{
									StringToUsart2("8");
								}
								else if (matrix[2][2]==1)
								{
									StringToUsart2("9");
								}
								druk = 1;
						}
					}
			//dendering voorkomen
			if (yur == 1000000)
				{
					yur = 0;
					if (matrix[i][j] == 0)
						{
							druk = 0;
						}
				}
			else
				{
					yur++;
				}
			}
			// Deactivate the row
			GPIOA->ODR &= ~(1 << (7 + i)); // Set the ith row pin low
    }
}

void potmeter()
{
				adValue = GetAdValue(); 

				// Bepaal stand op basis van de adValue
				if (adValue < 1)
				{
						outputValue = 0;
				}
				else if (adValue > 0 && adValue < 1310)
				{
						outputValue = 1;
				}
				else if (adValue >= 1310 && adValue < 2105)
				{
						outputValue = 2;
				}
				else if (adValue >= 2105 && adValue < 2730)
				{
						outputValue = 3;
				}
				else if (adValue >= 2730 && adValue < 3035)
				{
						outputValue = 4;
				}
				else if (adValue >= 3035 && adValue < 3250)
				{
						outputValue = 5;
				}
				else if (adValue >= 3250 && adValue < 3570)
				{
						outputValue = 6;
				}
				else if (adValue >= 3570 && adValue < 3750)
				{
						outputValue = 7;
				}
				else if (adValue >= 3760 && adValue < 3850)
				{
						outputValue = 8;
				}
				else if (adValue >= 3850 && adValue < 3900)
				{
						outputValue = 9;
				}
				else if (adValue >= 3900)
				{
						outputValue = 10;
				}
				
				WaitForMs(100);
			
			if( outputValueVorige != outputValue)
			{
				switch (outputValue)
        {
            case 0:
                StringToUsart2("A");
                break;
            case 1:
                StringToUsart2("B");
                break;
            case 2:
                StringToUsart2("C");
                break;
            case 3:
                StringToUsart2("D");
                break;
            case 4:
                StringToUsart2("E");
                break;
            case 5:
                StringToUsart2("F");
                break;
            case 6:
                StringToUsart2("G");
                break;
            case 7:
                StringToUsart2("H");
                break;
            case 8:
                StringToUsart2("I");
                break;
            case 9:
                StringToUsart2("J");
                break;
            case 10:
                StringToUsart2("K");
                break;
        }
			}
				outputValueVorige = outputValue;
}


void joystick()
{
	 if(BovenActive())
			{
				while (BovenActive())
				{
					StringToUsart2("U");
					WaitForMs(250);
				}					 
				
			}
			
	if(OnderActive())
			{
				while (OnderActive())
				{
					StringToUsart2("O");
					WaitForMs(250);
				}					 
				
			}
			
	if(LinksActive())
			{
				while (LinksActive())
				{
					StringToUsart2("L");
					WaitForMs(250);
				}					 
			}
			
	if(RechtsActive())
			{
				while (RechtsActive())
				{
					StringToUsart2("R");
					WaitForMs(250);
				}					 
			}
}
int main(void)
{
	// Initialisaties.
	SystemClock_Config();
	InitIo();
	InitUsart2(9600);
	InitAd();
	InitButtons();
	initRows();
  initCols();
	
	int matrix[ROWS][COLS];
	int wacht = 0;
	bool controleloop = 0;
	static char dataText[101];
	
	StringToUsart2("Reboot\r\n");
	
	while (controleloop == 0)
	{
		StringToUsart2("z");
		scanMatrix(matrix);
		if(matrix[0][0]==1)
			{
				controleloop = 1;
				FirstRun = 1;
			}
		WaitForMs(250);
	}
	
	// Oneindige lus starten.
	while (1)
	{	
		if (wacht == 25000)
		{
		potmeter();
		wacht = 0;
		}
		else
		{
			wacht++;
		}
		scanMatrix(matrix);
		 joystick();
	}

	
	// Terugkeren zonder fouten... (unreachable).
	return 0;
}

// Functie om extra IO's te initialiseren.
void InitIo(void)
{

}

// Handler die iedere 1ms afloopt. Ingesteld met SystemCoreClockUpdate() en SysTick_Config().
void SysTick_Handler(void)
{
	ticks++;
}

// Wachtfunctie via de SysTick.
void WaitForMs(uint32_t timespan)
{
	uint32_t startTime = ticks;
	
	while(ticks < startTime + timespan);
}

// Klokken instellen. Deze functie niet wijzigen, tenzij je goed weet wat je doet.
void SystemClock_Config(void)
{
	RCC->CR |= RCC_CR_HSITRIM_4;														// HSITRIM op 16 zetten, dit is standaard (ook na reset).
	RCC->CR  |= RCC_CR_HSION;																// Internal high speed oscillator enable (8MHz)
	while((RCC->CR & RCC_CR_HSIRDY) == 0);									// Wacht tot HSI zeker ingeschakeld is
	
	RCC->CFGR &= ~RCC_CFGR_SW;															// System clock op HSI zetten (SWS is status geupdatet door hardware)	
	while((RCC->CFGR & RCC_CFGR_SWS) != RCC_CFGR_SWS_HSI);	// Wachten to effectief HSI in actie is getreden
	
	RCC->CR &= ~RCC_CR_PLLON;																// Eerst PLL uitschakelen
	while((RCC->CR & RCC_CR_PLLRDY) != 0);									// Wacht tot PLL zeker uitgeschakeld is
	
	RCC->CFGR |= RCC_CFGR_PLLSRC_HSI_PREDIV;								// 01: HSI/PREDIV selected as PLL input clock
	RCC->CFGR2 |= RCC_CFGR2_PREDIV_DIV2;										// prediv = /2		=> 4MHz
	RCC->CFGR |= RCC_CFGR_PLLMUL12;													// PLL multiplied by 12 => 48MHz
	
	FLASH->ACR |= FLASH_ACR_LATENCY;												//  meer dan 24 MHz, dus latency op 1 (p 67)
	
	RCC->CR |= RCC_CR_PLLON;																// PLL inschakelen
	while((RCC->CR & RCC_CR_PLLRDY) == 0);									// Wacht tot PLL zeker ingeschakeld is

	RCC->CFGR |= RCC_CFGR_SW_PLL; 													// PLLCLK selecteren als SYSCLK (48MHz)
	while((RCC->CFGR & RCC_CFGR_SWS) != RCC_CFGR_SWS_PLL);	// Wait until the PLL is switched on
		
	RCC->CFGR |= RCC_CFGR_HPRE_DIV1;												// SYSCLK niet meer delen, dus HCLK = 48MHz
	RCC->CFGR |= RCC_CFGR_PPRE_DIV1;												// HCLK niet meer delen, dus PCLK = 48MHz	
	
	SystemCoreClockUpdate();																// Nieuwe waarde van de core frequentie opslaan in SystemCoreClock variabele
	SysTick_Config(48000);																	// Interrupt genereren. Zie core_cm0.h, om na ieder 1ms een interrupt 
																													// te hebben op SysTick_Handler()
}
