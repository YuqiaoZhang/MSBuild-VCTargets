#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/wait.h>

#include <string.h>

#include <stdio.h>
#include <assert.h>

#include <stdint.h>
#include <byteswap.h>

#include <uuid/uuid.h>

#include <string>

bool iConv_UTF16ToUTF8(uint16_t const *pInBuf, size_t *pInCharsLeft, char *pOutBuf, size_t *pOutCharsLeft);

int main(int argc, char **argv)
{

    char tmpbashPath[256];
    {
        char *pathToTool = NULL;

        if (argc > 3)
        {
            if (::strcmp(argv[argc - 3], "/c") == 0)
            {
                pathToTool = argv[argc - 2];
            }
        }

        if (pathToTool == NULL)
        {
            ::printf("/c [command-line]   :Command to be tracked (must be last argument).\n");
            return -1;
        }

        char *responseFileSwitch = NULL;
        if (argc > 2)
        {
            if (argv[argc - 1][0] == '@')
            {
                responseFileSwitch = argv[argc - 1] + 1;
            }
        }

        if (responseFileSwitch == NULL)
        {
            ::printf("missing responsible file\n");
            return -1;
        }

        char responseFileCommands[4096];
        {
            uint16_t responseFileCommands_Buffer[4096];
            int responseFile = ::open64(responseFileSwitch, O_RDONLY);

            if (responseFile == -1)
            {
                ::printf("Fail to open responseFile:\n%s\n", responseFileSwitch);
                return -1;
            }

            ssize_t nbytesread = ::read(responseFile, responseFileCommands_Buffer, sizeof(uint16_t) * 4096);
            assert(nbytesread < (sizeof(uint16_t) * 4096));
            ::close(responseFile);

            //Handle BOM
            uint16_t *responseFileCommands_UTF16;
            if (responseFileCommands_Buffer[0] == 0XFEFF)
            {
                responseFileCommands_UTF16 = responseFileCommands_Buffer + 1;
            }
            else if (responseFileCommands_Buffer[0] == 0XFFFE)
            {
                for (int _c = 1; _c < (nbytesread / sizeof(uint16_t)); ++_c)
                {
                    responseFileCommands_Buffer[_c] = (bswap_16(responseFileCommands_Buffer[_c]));
                }
                responseFileCommands_UTF16 = responseFileCommands_Buffer + 1;
            }
            else
            {
                responseFileCommands_UTF16 = responseFileCommands_Buffer;
            }

            size_t InCharsLeft = (nbytesread / sizeof(uint16_t));
            size_t OutCharsLeft = 4096;
            bool iConv_Success = iConv_UTF16ToUTF8(responseFileCommands_UTF16, &InCharsLeft, responseFileCommands, &OutCharsLeft);
            if (!iConv_Success)
            {
                ::printf("Fail to convert responseFileCommands from UTF16 to UTF8:\n%s\n", responseFileCommands);
                return -1;
            }
        }

        //char tmpbashPath[256];
        {
            tmpbashPath[0] = '/';
            tmpbashPath[1] = 't';
            tmpbashPath[2] = 'm';
            tmpbashPath[3] = 'p';
            tmpbashPath[4] = '/';
            tmpbashPath[5] = 't';
            tmpbashPath[6] = 'm';
            tmpbashPath[7] = 'p';

            char *_str_uu = tmpbashPath + 8;
            {
                uuid_t _out_uu;
                ::uuid_generate_time_safe(_out_uu);

                int _c_str = 0;
                for (int _c_uu = 0; _c_uu < 16; ++_c_uu)
                {
                    uint8_t val2 = _out_uu[_c_uu];
                    uint8_t val2_1 = (val2 >> 4);
                    uint8_t val2_2 = (val2 & (0XF));

                    uint8_t val_vec[2] = {val2_1, val2_2};

                    for (int _i_val_vec = 0; _i_val_vec < 2; ++_i_val_vec)
                    {
                        switch (val_vec[_i_val_vec])
                        {
                        case 0:
                        {
                            _str_uu[_c_str] = '0';
                        }
                        break;
                        case 1:
                        {
                            _str_uu[_c_str] = '1';
                        }
                        break;
                        case 2:
                        {
                            _str_uu[_c_str] = '2';
                        }
                        break;
                        case 3:
                        {
                            _str_uu[_c_str] = '3';
                        }
                        break;
                        case 4:
                        {
                            _str_uu[_c_str] = '4';
                        }
                        break;
                        case 5:
                        {
                            _str_uu[_c_str] = '5';
                        }
                        break;
                        case 6:
                        {
                            _str_uu[_c_str] = '6';
                        }
                        break;
                        case 7:
                        {
                            _str_uu[_c_str] = '7';
                        }
                        break;
                        case 8:
                        {
                            _str_uu[_c_str] = '8';
                        }
                        break;
                        case 9:
                        {
                            _str_uu[_c_str] = '9';
                        }
                        break;
                        case 10:
                        {
                            _str_uu[_c_str] = 'a';
                        }
                        break;
                        case 11:
                        {
                            _str_uu[_c_str] = 'b';
                        }
                        break;
                        case 12:
                        {
                            _str_uu[_c_str] = 'c';
                        }
                        break;
                        case 13:
                        {
                            _str_uu[_c_str] = 'd';
                        }
                        break;
                        case 14:
                        {
                            _str_uu[_c_str] = 'e';
                        }
                        break;
                        case 15:
                        {
                            _str_uu[_c_str] = 'f';
                        }
                        break;
                        default:
                        {
                            _str_uu[_c_str] = '#';
                        }
                        }
                        ++_c_str;
                    }
                }
            }
            tmpbashPath[7 + 32 + 1] = '.';
            tmpbashPath[7 + 32 + 2] = 's';
            tmpbashPath[7 + 32 + 3] = 'h';
            tmpbashPath[7 + 32 + 4] = '\0';
        }

        int bashFile;
        {
            bashFile = ::open64(tmpbashPath, O_WRONLY | O_CREAT | O_EXCL, S_IRWXU | S_IRGRP | S_IXGRP | S_IROTH | S_IXOTH);
            if (bashFile == -1)
            {
                ::printf("Fail to create temp file:\n%s\n", tmpbashPath);
                return -1;
            }

            std::string bashFileContent = "#! /bin/sh\n";
            bashFileContent += pathToTool;
            bashFileContent += ' ';
            bashFileContent += responseFileCommands;
            bashFileContent += '\n';

            ::write(bashFile, bashFileContent.c_str(), sizeof(char) * bashFileContent.length());

            ::close(bashFile);
        }
    }

    //We just execute now!

    //To DO:
    //File Tracker

    pid_t _pid_child;

    if ((_pid_child = ::fork()) == 0)
    {
        char _bash_arg1[] = {"bash"};
        char _bash_arg2[] = {"-c"};
        char *_bash_argv[] = {_bash_arg1, _bash_arg2, tmpbashPath, NULL};
        return ::execvp(_bash_arg1, _bash_argv);
    }
    else
    {
        int status;
        ::waitpid(_pid_child, &status, 0);

        int _retval;
        if (WIFEXITED(status))
        {
            _retval = (WEXITSTATUS(status));
        }
        else
        {
            _retval = -1;
        }

        ::unlink(tmpbashPath);

        return _retval;
    }
}

bool iConv_UTF16ToUTF8(uint16_t const *pInBuf, size_t *pInCharsLeft, char *pOutBuf, size_t *pOutCharsLeft)
{
    while ((*pInCharsLeft) >= 1)
    {
        uint32_t ucs4code = 0; //Accumulator

        //UTF-16 To UCS-4
        if ((*pInBuf) >= 0XD800U && (*pInBuf) <= 0XDBFFU) //110110xxxxxxxxxx
        {
            if ((*pInCharsLeft) >= 2)
            {
                ucs4code += (((*pInBuf) - 0XD800U) << 10U); //Accumulate

                ++pInBuf;
                --(*pInCharsLeft);

                if ((*pInBuf) >= 0XDC00U && (*pInBuf) <= 0XDFFF) //110111xxxxxxxxxx
                {
                    ucs4code += ((*pInBuf) - 0XDC00U); //Accumulate

                    ++pInBuf;
                    --(*pInCharsLeft);
                }
                else
                {
                    return false;
                }

                ucs4code += 0X10000U;
            }
            else
            {
                return false;
            }
        }
        else
        {
            ucs4code += (*pInBuf); //Accumulate

            ++pInBuf;
            --(*pInCharsLeft);
        }

        //UCS-4 To UTF-16
        if (ucs4code < 128U) //0XXX XXXX
        {
            if ((*pOutCharsLeft) >= 1)
            {
                (*pOutBuf) = static_cast<uint8_t>(ucs4code);

                ++pOutBuf;
                --(*pOutCharsLeft);
            }
            else
            {
                return false;
            }
        }
        else if (ucs4code < 2048U) //110X XXXX 10XX XXXX
        {
            if ((*pOutCharsLeft) >= 2)
            {
                (*pOutBuf) = static_cast<uint8_t>(((ucs4code & 0X7C0U) >> 6U) + 192U);

                ++pOutBuf;
                --(*pOutCharsLeft);

                (*pOutBuf) = static_cast<uint8_t>((ucs4code & 0X3FU) + 128U);

                ++pOutBuf;
                --(*pOutCharsLeft);
            }
            else
            {
                return false;
            }
        }
        else if (ucs4code < 0X10000U) //1110 XXXX 10XX XXXX 10XX XXXX
        {
            if ((*pOutCharsLeft) >= 3)
            {
                (*pOutBuf) = static_cast<uint8_t>(((ucs4code & 0XF000U) >> 12U) + 224U);

                ++pOutBuf;
                --(*pOutCharsLeft);

                (*pOutBuf) = static_cast<uint8_t>(((ucs4code & 0XFC0U) >> 6U) + 128U);

                ++pOutBuf;
                --(*pOutCharsLeft);

                (*pOutBuf) = static_cast<uint8_t>((ucs4code & 0X3FU) + 128U);

                ++pOutBuf;
                --(*pOutCharsLeft);
            }
            else
            {
                return false;
            }
        }
        else if (ucs4code < 0X200000U) //1111 0XXX 10XX XXXX 10XX XXXX 10XX XXXX
        {
            if ((*pOutCharsLeft) >= 4)
            {
                (*pOutBuf) = static_cast<uint8_t>(((ucs4code & 0X1C0000U) >> 18U) + 240U);

                ++pOutBuf;
                --(*pOutCharsLeft);

                (*pOutBuf) = static_cast<uint8_t>(((ucs4code & 0X3F000U) >> 12U) + 128U);

                ++pOutBuf;
                --(*pOutCharsLeft);

                (*pOutBuf) = static_cast<uint8_t>(((ucs4code & 0XFC0U) >> 6U) + 128U);

                ++pOutBuf;
                --(*pOutCharsLeft);

                (*pOutBuf) = static_cast<uint8_t>((ucs4code & 0X3FU) + 128U);

                ++pOutBuf;
                --(*pOutCharsLeft);
            }
            else //ucs4code >= 0X200000U
            {
                return false;
            }
        }
    }

    return true;
}