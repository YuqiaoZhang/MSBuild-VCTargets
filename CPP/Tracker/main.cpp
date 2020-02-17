#include <unistd.h>
#include <sys/signal.h>
#include <sys/wait.h>
#include <sys/ptrace.h>

#include <assert.h>

#include <bits/wordsize.h>
#include <sys/syscall.h>

#include <sys/reg.h>

#include <errno.h>
#include <string.h>
#include <limits.h>
#include <stdio.h>
#include <stddef.h>
#include <stdint.h>
#include <fcntl.h>

void ptrace_get_syscall_number(pid_t pid, uint64_t *nr);

void ptrace_get_syscall_args(pid_t pid, uint64_t *arg1);
void ptrace_get_syscall_args(pid_t pid, uint64_t *arg1, uint64_t *arg2);
void ptrace_get_syscall_args(pid_t pid, uint64_t *arg1, uint64_t *arg2, uint64_t *arg3);

void ptrace_read_pid_string(pid_t pid, uint64_t src, char *dest, size_t n);

int main(int argc, char **argv)
{
    pid_t pid;
    if ((pid = ::fork()) == 0)
    {
        // http://man7.org/linux/man-pages/man2/ptrace.2.html
        // If the PTRACE_O_TRACEEXEC option is not in effect, all successful
        // calls to execve(2) by the traced process will cause it to be sent a
        // SIGTRAP signal, giving the parent a chance to gain control before the
        // new program begins execution.
        ::ptrace(PTRACE_TRACEME, 0L, 0L, 0L);

        //::kill(::getpid(), SIGTRAP);

        //::open("XXXX", O_RDWR);

        return ::execlp("cat", "cat", "/usr/include/stdio.h", NULL);
    }
    else
    {
        int status;
        ::waitpid(pid, &status, 0);
        assert(WIFSTOPPED(status)); //Stop by SIGTRAP caused by execve(2)

        //ptrace only works when tracee is stopped?
        int i = ::ptrace(PTRACE_SETOPTIONS, pid, 0L, PTRACE_O_TRACESYSGOOD);

        for (int _7 = 0; _7 < 150; ++_7)
        {
            // http://man7.org/linux/man-pages/man2/ptrace.2.html
            // Restart the stopped tracee as for PTRACE_CONT.
            i = ::ptrace(PTRACE_SYSCALL, pid, 0L, 0L); //continue
            auto str2 = strerror(errno);

            //Syscall Entry
            //int status;
            auto p2 = ::waitpid(pid, &status, 0); //Sync
            bool isstopped = (WIFSTOPPED(status));
            auto stopsig = (WSTOPSIG(status));

            if ((WIFSTOPPED(status)) && ((WSTOPSIG(status)) == (SIGTRAP | 0X80)))
            {
                uint64_t systemcallnumber_entry;
                ::ptrace_get_syscall_number(pid, &systemcallnumber_entry);

                switch (systemcallnumber_entry)
                {
                case SYS_open:
                {
                    uint64_t arg1;
                    uint64_t arg2_flags;
                    ::ptrace_get_syscall_args(pid, &arg1, &arg2_flags);

                    char pathname[4096];
                    ::ptrace_read_pid_string(pid, arg1, pathname, 4096);

                    char accmode[16];
                    switch (arg2_flags & O_ACCMODE)
                    {
                    case O_RDONLY:
                        ::strncpy(accmode, "O_RDONLY", 16);
                        break;
                    case O_WRONLY:
                        ::strncpy(accmode, "O_WRONLY", 16);
                        break;
                    case O_RDWR:
                        ::strncpy(accmode, "O_RDWR", 16);
                        break;
                    default:
                        break;
                    }

                    printf("track - %s - %s \n", pathname, accmode);
                }
                break;
                case SYS_openat:
                {
                    uint64_t arg1_dirfd;
                    uint64_t arg2_pathname;
                    uint64_t arg3_flags;
                    ::ptrace_get_syscall_args(pid, &arg1_dirfd, &arg2_pathname, &arg3_flags);

                    char pathname[4096];
                    ::ptrace_read_pid_string(pid, arg2_pathname, pathname, 4096);

                    char accmode[16];
                    switch (arg3_flags & O_ACCMODE)
                    {
                    case O_RDONLY:
                        ::strncpy(accmode, "O_RDONLY", 16);
                        break;
                    case O_WRONLY:
                        ::strncpy(accmode, "O_WRONLY", 16);
                        break;
                    case O_RDWR:
                        ::strncpy(accmode, "O_RDWR", 16);
                        break;
                    default:
                        break;
                    }

                    printf("track - %s - %s \n", pathname, accmode);
                }
                break;
                default:
                    break;
                }

                // Restart the stopped tracee as for PTRACE_CONT.
                i = ::ptrace(PTRACE_SYSCALL, pid, 0L, 0L);

                //Syscall Exit //Some Syscall(such as exec) not return?
                p2 = ::waitpid(pid, &status, 0);
                bool isstopped = (WIFSTOPPED(status));
                bool isexited = (WIFEXITED(status));

                if ((WIFEXITED(status)))
                {
                    break;
                }

                uint64_t systemcallnumber_exit;
                ::ptrace_get_syscall_number(pid, &systemcallnumber_exit);

                assert(systemcallnumber_entry == systemcallnumber_exit);
            }
            else if ((WIFEXITED(status)) || ((WIFSIGNALED(status))))
            {
                break;
            }
        }
        
        return 0;
    }
}

// http://man7.org/linux/man-pages/man2/syscall.2.html

void ptrace_get_syscall_number(pid_t pid, uint64_t *nr)
{

#ifdef __x86_64__
    (*nr) = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * ORIG_RAX, 0L); //ORIG_RAX:System call # //RAX: Ret val
#elif defined(__i386__)
#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif
}

void ptrace_get_syscall_args(pid_t pid, uint64_t *arg1)
{

#ifdef __x86_64__
    (*arg1) = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * RDI, 0L);
#elif defined(__i386__)

#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif
}

void ptrace_get_syscall_args(pid_t pid, uint64_t *arg1, uint64_t *arg2)
{

#ifdef __x86_64__
    (*arg2) = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * RSI, 0L);
#elif defined(__i386__)

#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif

    ptrace_get_syscall_args(pid, arg1);
}

void ptrace_get_syscall_args(pid_t pid, uint64_t *arg1, uint64_t *arg2, uint64_t *arg3)
{

#ifdef __x86_64__
    (*arg3) = ::ptrace(PTRACE_PEEKUSER, pid, (__WORDSIZE / 8) * RDX, 0L);
#elif defined(__i386__)

#elif defined(__arm__)
#elif defined(__aarch64__)
#else
#error Unknown Architecture 未知的架构
#endif

    ptrace_get_syscall_args(pid, arg1, arg2);
}

void ptrace_read_pid_string(pid_t pid, uint64_t src, char *dest, size_t n)
{
    for (int _w = 0; _w < ((n > 0) ? ((n - 1) / (__WORDSIZE / CHAR_BIT)) : 0); ++_w)
    {
        auto val4 = ::ptrace(PTRACE_PEEKTEXT, pid, src + (__WORDSIZE / CHAR_BIT) * _w, 0L);

        bool meetzeroterm = false;
        for (int _c = 0; _c < (__WORDSIZE / CHAR_BIT); ++_c)
        {
            char val = reinterpret_cast<char *>(&val4)[_c];
            dest[(__WORDSIZE / CHAR_BIT) * _w + _c] = val;
            if (val == '\0')
            {
                meetzeroterm = true;
                break;
            }
        }

        if (meetzeroterm)
        {
            break;
        }
    }
}